using LF.Compiler.C.Types;
using LF.Compiler.Reader;

namespace LF.Compiler.C.Functions;

public class LFCExternFunction : LFCFunction
{
    public LFCExternFunction(
        string name,
        LFCTypeToken returnType,
        LFCFunctionParameter[] parameters,
        LFReaderCodeSection section,
        LFSourcePosition position,
        bool isInstance = false,
        LFCTypeToken? instanceTypeName = null) : base(
        name,
        returnType,
        parameters,
        Array.Empty<LFCTypeToken>(),
        section,
        isInstance,
        instanceTypeName,
        position
    ) { }

    public override void Compile(LFReaderResult result) { }

    public override string GetPrintableName()
    {
        return Name;
    }

    public override LFCFunction CreateInstance(LFCTypeToken instanceType)
    {
        List<LFCFunctionParameter> parameters = new List<LFCFunctionParameter>(Parameters);
        parameters.Insert(0, new LFCFunctionParameter("this", new LFCTypeToken(instanceType.Name + "*", instanceType.TypeArgs)));

        return new LFCExternFunction(
            LFCStructureType.GetFunctionName(instanceType.Name, Name),
            ReturnTypeName,
            parameters.ToArray(),
            ContainingSection,
            Position,
            true,
            instanceType
        );
    }

    public override LFCFunction CreateOperator(LFCTypeToken instanceType)
    {
        string name = LFCStructureType.GetFunctionName(instanceType.Name, Name) + "_" + string.Join('_', Parameters.Select(x => x.TypeName.ToString()));

        return new LFCExternFunction(
            name,
            ReturnTypeName,
            Parameters.ToArray(),
            ContainingSection,
            Position
        );
    }

    public override void Compile(LFReaderResult result, LFCType[] typeParameters)
    {
        throw new LFCParserException("Can not compile extern function with type parameters", Position);
    }

    public override string ToString()
    {
        return $"extern {base.ToString()}";
    }
}