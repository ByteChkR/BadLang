using System.Text;

using LF.Compiler.C.Functions;
using LF.Compiler.Logging;
using LF.Compiler.Reader;

namespace LF.Compiler.C.Types;

public static class LFCAssemblyManifestExtensions
{
    //ParameterTable (sizeof(Count) + sizeof(ParameterEntry) * Count)
    //  Count: int
    //  ParameterEntry 0
    //  ..

    private const int TYPE_ENTRY_SIZE = 16;

    //TypeEntry 16 Bytes
    //  Type Name Offset: int
    //  Base Type Offset: int
    //  MethodTable Offset: int
    //  FieldTable Offset: int

    private const int FIELD_ENTRY_SIZE = 8;

    //FieldEntry 8 Bytes
    //  Field Name Offset: int
    //  Field Type Offset: int

    private const int METHOD_ENTRY_SIZE = 12;

    //MethodEntry 12 Bytes
    //  Method Name Offset: int
    //  Return Type Offset: int
    //  ParameterTable Offset: int


    private const int PARAMETER_ENTRY_SIZE = 8;

    private static void Patch(this List<byte> data, int start, byte[] buf)
    {
        for (int i = 0; i < buf.Length; i++)
        {
            data[start + i] = buf[i];
        }
    }

    private static void GetUsedTypes(LFCType type, LFCTypeDatabase db, List<LFCType> usedTypes)
    {
        if (type.BaseType != null && !usedTypes.Contains(type.BaseType))
        {
            usedTypes.Add(type.BaseType);
            GetUsedTypes(type.BaseType, db, usedTypes);
        }

        foreach ((string name, LFCType type) field in type.GetFields())
        {
            if (!usedTypes.Contains(field.type))
            {
                usedTypes.Add(field.type);
                GetUsedTypes(field.type, db, usedTypes);
            }
        }

        foreach ((string name, LFCFunctionType type) method in type.GetMethods())
        {
            if (method.type.SourceFunction == null)
            {
                throw new Exception("Invalid Method Type");
            }

            LFCType[] types = type.Name.TypeArgs.Select(x => db.GetType(x, LFSourcePosition.Unknown)).ToArray();

            LFCType returnType = db.GetType(method.type.ReturnTypeName, LFSourcePosition.Unknown);

            if (!usedTypes.Contains(returnType))
            {
                usedTypes.Add(returnType);
                GetUsedTypes(returnType, db, usedTypes);
            }

            foreach (LFCFunctionParameter parameter in method.type.SourceFunction.Parameters)
            {
                LFCTypeToken[] typeTokens = method.type.SourceFunction.InstanceTypeName?.TypeArgs ?? new LFCTypeToken[0];
                LFCType parameterType;
                if (typeTokens.Length != types.Length)
                {
                    parameterType = db.GetType(parameter.TypeName, LFSourcePosition.Unknown);
                }
                else
                {
                    parameterType = db.GetType(LFCScope.ResolveType(parameter.TypeName, LFCScope.CreateTypeMap(types, typeTokens)), LFSourcePosition.Unknown);
                }

                if (!usedTypes.Contains(parameterType))
                {
                    usedTypes.Add(parameterType);
                    GetUsedTypes(parameterType, db, usedTypes);
                }
            }
        }
    }

    private static byte[] SerializeTypeTable(LFReaderResult result)
    {
        //Find Size of Data
        LFCTypeDatabase db = LFCTypeDatabase.GetTypeSystem(result);
        LFCType[] types = db.Types.ToArray();
        int size = GetStaticManifestSize(db);
        Logger.Debug($"Static Manifest Size for '{result.Name}': {size} Bytes");
        Dictionary<LFCType, int> typeInfos = new Dictionary<LFCType, int>();
        List<byte> data = new List<byte>();

        Queue<Action<Dictionary<LFCType, int>>> onAfterSerialization = new Queue<Action<Dictionary<LFCType, int>>>();

        data.AddRange(BitConverter.GetBytes(types.Length));
        foreach (LFCType type in types)
        {
            typeInfos[type] = data.Count;
            Logger.Debug($"Serializing Type Info for '{type.Name}'");
            SerializeTypeInfo(type, db, data, onAfterSerialization);
        }

        List<LFCType> usedTypes = new List<LFCType>(types);
        foreach (LFCType type in types)
        {
            GetUsedTypes(type, db, usedTypes);
        }

        foreach (LFCType type in usedTypes)
        {
            typeInfos[type] = data.Count;
            Logger.Debug($"Serializing Type Info for '{type.Name}'");
            SerializeTypeInfo(type, db, data, onAfterSerialization);
        }

        while (onAfterSerialization.Count != 0)
        {
            onAfterSerialization.Dequeue()(typeInfos);
        }

        return data.ToArray();
    }

    private static void EmitGetter(this LFReaderCodeSection section, string getterName, LFReaderDataSection data, string element)
    {
        section.AddImport(data.Name);
        section.AddExport(getterName);
        section.CreateLabel(getterName, LFSourcePosition.Unknown);
        section.Emit(LFSourcePosition.Unknown, OpCodes.Push64, $"{data.Name}::{element}");
        section.Emit(LFSourcePosition.Unknown, OpCodes.Return);
    }

    public static void EmitAssemblyManifest(this LFReaderResult result)
    {
        byte[] typeTableData = SerializeTypeTable(result);
        LFReaderDataSection outputSection = result.CreateDataSection("__ASSEMBLY_MANIFEST__");
        outputSection.EmitData("ASSEMBLY_NAME", Encoding.UTF8.GetBytes(result.Name + '\0'));

        outputSection.EmitData("TYPE_SYSTEM", typeTableData);

        LFReaderCodeSection wrapperSection = result.CreateCodeSection("AssemblyManifest");
        wrapperSection.EmitGetter("GetAssemblyName", outputSection, "ASSEMBLY_NAME");
        wrapperSection.EmitGetter("GetTypeSystem", outputSection, "TYPE_SYSTEM");
    }

    private static void SerializeTypeInfo(LFCType type, LFCTypeDatabase db, List<byte> data, Queue<Action<Dictionary<LFCType, int>>> onAfterSerialization)
    {
        int nameIndex = data.Count;
        data.AddRange(BitConverter.GetBytes(0)); //Name

        onAfterSerialization.Enqueue(
            typeMap =>
            {
                Logger.Debug($"Patching '{type.Name}::NAME'");
                int off = data.Count;
                data.Patch(nameIndex, BitConverter.GetBytes(off));
                data.AddRange(Encoding.UTF8.GetBytes(type.Name.ToString() + '\0'));
            }
        );

        data.AddRange(BitConverter.GetBytes(0)); //Base Type

        int methodIndex = data.Count;
        data.AddRange(BitConverter.GetBytes(0)); //Method Table
        onAfterSerialization.Enqueue(
            typeMap =>
            {
                Logger.Debug($"Patching '{type.Name}::METHOD_TABLE'");
                int off = data.Count;
                data.Patch(methodIndex, BitConverter.GetBytes(off));
                SerializeMethodTable(type, db, data, onAfterSerialization);
            }
        );

        int fieldIndex = data.Count;
        data.AddRange(BitConverter.GetBytes(0)); //Field Table
        onAfterSerialization.Enqueue(
            typeMap =>
            {
                Logger.Debug($"Patching '{type.Name}::FIELD_TABLE'");
                int off = data.Count;
                data.Patch(fieldIndex, BitConverter.GetBytes(off));
                SerializeFieldTable(type, data, onAfterSerialization);
            }
        );
    }

    private static void SerializeMethodTable(LFCType type, LFCTypeDatabase db, List<byte> data, Queue<Action<Dictionary<LFCType, int>>> onAfterSerialization)
    {
        (string name, LFCFunctionType type)[] methods = type.GetMethods().ToArray();
        data.AddRange(BitConverter.GetBytes(methods.Length));
        foreach ((string name, LFCFunctionType type) method in methods)
        {
            int nameIndex = data.Count;
            data.AddRange(BitConverter.GetBytes(0)); //Name
            onAfterSerialization.Enqueue(
                typeMap =>
                {
                    int off = data.Count;
                    data.AddRange(Encoding.UTF8.GetBytes(method.name));
                    data.Patch(nameIndex, BitConverter.GetBytes(off));
                }
            );

            int returnIndex = data.Count;
            data.AddRange(BitConverter.GetBytes(0)); //Return Type
            onAfterSerialization.Enqueue(
                typeMap => { data.Patch(returnIndex, BitConverter.GetBytes(typeMap[db.GetType(method.type.ReturnTypeName, LFSourcePosition.Unknown)])); }
            );

            int parameterIndex = data.Count;
            data.AddRange(BitConverter.GetBytes(0)); //Parameter Table
            onAfterSerialization.Enqueue(
                typeMap =>
                {
                    int off = data.Count;
                    data.Patch(parameterIndex, BitConverter.GetBytes(off));
                    LFCType[] pTypes = type.Name.TypeArgs.Select(x => db.GetType(x, LFSourcePosition.Unknown)).ToArray();
                    SerializeParameterTable(method.type.SourceFunction!, pTypes, data, db, onAfterSerialization);
                }
            );
        }
    }

    private static void SerializeParameterTable(
        LFCFunction function,
        LFCType[] pTypes,
        List<byte> data,
        LFCTypeDatabase db,
        Queue<Action<Dictionary<LFCType, int>>> onAfterSerialization)
    {
        LFCFunctionParameter[] parameters = function.Parameters.ToArray();
        data.AddRange(BitConverter.GetBytes(parameters.Length));
        foreach (LFCFunctionParameter parameter in parameters)
        {
            int nameIndex = data.Count;
            data.AddRange(BitConverter.GetBytes(0)); //Name
            onAfterSerialization.Enqueue(
                typeMap =>
                {
                    int off = data.Count;
                    data.AddRange(Encoding.UTF8.GetBytes(parameter.Name));
                    data.Patch(nameIndex, BitConverter.GetBytes(off));
                }
            );

            int typeIndex = data.Count;
            data.AddRange(BitConverter.GetBytes(0)); //Type
            onAfterSerialization.Enqueue(
                typeMap =>
                {
                    data.Patch(
                        typeIndex,
                        BitConverter.GetBytes(
                            typeMap[db.GetType(
                                LFCScope.ResolveType(parameter.TypeName, LFCScope.CreateTypeMap(pTypes, function.InstanceTypeName!.Value.TypeArgs)),
                                LFSourcePosition.Unknown
                            )]
                        )
                    );
                }
            );
        }
    }

    private static void SerializeFieldTable(LFCType type, List<byte> data, Queue<Action<Dictionary<LFCType, int>>> onAfterSerialization)
    {
        (string name, LFCType type)[] fields = type.GetFields().ToArray();
        data.AddRange(BitConverter.GetBytes(fields.Length));
        foreach ((string name, LFCType type) field in fields)
        {
            Logger.Debug($"Serializing Field '{type.Name}::{field.name}'");
            int nameIndex = data.Count;
            data.AddRange(BitConverter.GetBytes(0)); //Name
            onAfterSerialization.Enqueue(
                typeMap =>
                {
                    int off = data.Count;
                    Logger.Debug($"Patching Field Name '{type.Name}::{field.name}'");
                    data.AddRange(Encoding.UTF8.GetBytes(field.name));
                    data.Patch(nameIndex, BitConverter.GetBytes(off));
                }
            );
            int typeIndex = data.Count;
            data.AddRange(BitConverter.GetBytes(0)); //Type
            onAfterSerialization.Enqueue(
                typeMap =>
                {
                    Logger.Debug($"Patching Field Type '{type.Name}::{field.name}'");
                    data.Patch(typeIndex, BitConverter.GetBytes(typeMap[field.type]));
                }
            );
        }
    }


    private static int GetStaticManifestSize(LFCTypeDatabase db)
    {
        LFCType[] types = db.Types.ToArray();

        int size = GetManifestTableSize(types.Length);

        foreach (LFCType type in types)
        {
            (string name, LFCType type)[] fields = type.GetFields().ToArray();
            (string name, LFCFunctionType type)[] methods = type.GetMethods().ToArray();
            size += GetFieldTableSize(fields.Length);
            size += GetMethodTableSize(methods.Length);
            foreach ((string name, LFCFunctionType type) method in methods)
            {
                size += GetParameterTableSize(method.type.ParameterCount);
            }
        }

        return size;
    }

    //Data
    //  Type Entry Count: int
    //  Type Entry 0
    //  ..
    //  MethodTable(TypeEntry 0)
    //  ..
    //  FieldTable(TypeEntry 0)
    //  ..
    //  TypeName(TypeEntry 0)
    //  ..


    private static int GetManifestTableSize(int count)
    {
        return sizeof(int) + TYPE_ENTRY_SIZE * count;
    }

    //ManifestTable (sizeof(Count) + sizeof(TypeEntry) * Count) Bytes
    //  Count: int
    //  TypeEntry 0
    //  ..


    private static int GetMethodTableSize(int count)
    {
        return sizeof(int) + METHOD_ENTRY_SIZE * count;
    }

    //MethodTable (sizeof(Count) + sizeof(MethodEntry) * Count) Bytes
    //  Count: int
    //  MethodEntry 0
    //  ..

    private static int GetFieldTableSize(int count)
    {
        return sizeof(int) + FIELD_ENTRY_SIZE * count;
    }

    //FieldTable (sizeof(Count) + sizeof(FieldEntry) * Count) Bytes
    //  Count: int
    //  FieldEntry 0
    //  ..

    private static int GetParameterTableSize(int count)
    {
        return sizeof(int) + PARAMETER_ENTRY_SIZE * count;
    }

    //ParameterEntry 8 Bytes
    //  Parameter Name Offset: int
    //  Parameter Type Offset: int
}