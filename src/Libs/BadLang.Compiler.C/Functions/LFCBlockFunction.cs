using LF.Compiler.C.Expressions;
using LF.Compiler.C.Types;
using LF.Compiler.Logging;
using LF.Compiler.Reader;

namespace LF.Compiler.C.Functions;

public class LFCBlockFunction : LFCFunction
{
    private readonly LFCExpression[] m_Block;

    public LFCBlockFunction(
        string name,
        LFCTypeToken returnType,
        LFCFunctionParameter[] parameters,
        LFCTypeToken[] typeParameters,
        LFCExpression[] block,
        LFReaderCodeSection section,
        LFSourcePosition position,
        bool isInstance = false,
        LFCTypeToken? instanceTypeName = null) : base(name, returnType, parameters, typeParameters, section, isInstance, instanceTypeName, position)
    {
        m_Block = block;
    }

    private string ToString(LFCType[] typeParams)
    {
        if (typeParams.Length == 0)
        {
            return ToString();
        }

        List<LFCTypeToken> types = TypeParameters.ToList();
        if (InstanceTypeName != null)
        {
            types = InstanceTypeName.Value.TypeArgs.Concat(TypeParameters).ToList();
        }

        int rIndex = types.IndexOf(ReturnTypeName);
        LFCTypeToken returnType = ReturnTypeName;
        if (rIndex != -1)
        {
            returnType = typeParams[rIndex].Name;
        }

        return
            $"{returnType} {GetPrintableName()}{(typeParams.Length == 0 ? "" : $"<{string.Join(", ", typeParams.Select(x => x.ToString()))}>")}({string.Join(", ", Parameters.Select(p => p.ToString(types.ToList(), typeParams)))})";
    }

    public override void Compile(LFReaderResult result, LFCType[] typeParameters)
    {
        string funcName = GetFunctionLabel(Name, typeParameters);
        if (result.HasLabel(funcName, ContainingSection))
        {
            Logger.Debug($"Skip Compilation of Function: {ToString(typeParameters)}");

            return;
        }

        Logger.Info($"Compiling Function: {ToString(typeParameters)}");
        ContainingSection.CreateLabel(funcName, Position);
        if (AutoExport)
        {
            ContainingSection.AddExport(funcName);
        }

        LFCScope scope = CreateScope(LFCTypeDatabase.GetTypeSystem(result), typeParameters);
        LFCExpressionCompileInput input = new LFCExpressionCompileInput(result, ContainingSection, scope, false);
        foreach (LFCExpression expression in m_Block)
        {
            expression.Compile(input).DiscardResult();
        }

        scope.Return(ContainingSection, Position);
        ContainingSection.Emit(Position, OpCodes.Return);
    }

    public override LFCFunction CreateInstance(LFCTypeToken instanceType)
    {
        string name = LFCStructureType.GetFunctionName(instanceType.Name, Name);
        List<LFCFunctionParameter> parameters = new List<LFCFunctionParameter>(Parameters);
        parameters.Insert(0, new LFCFunctionParameter("this", new LFCTypeToken(instanceType.Name + "*", instanceType.TypeArgs)));

        return new LFCBlockFunction(name, ReturnTypeName, parameters.ToArray(), TypeParameters.ToArray(), m_Block, ContainingSection, Position, true, instanceType);
    }

    public override LFCFunction CreateOperator(LFCTypeToken instanceType)
    {
        string name = LFCStructureType.GetFunctionName(instanceType.Name, Name) + "_" + string.Join('_', Parameters.Select(x => x.TypeName.ToString()));

        return new LFCBlockFunction(name, ReturnTypeName, Parameters.ToArray(), TypeParameters.ToArray(), m_Block, ContainingSection, Position);
    }

    private LFCScope CreateScope(LFCTypeDatabase db, LFCType[]? typeArgs = null)
    {
        if (HasTypeParameters && (typeArgs == null || typeArgs.Length != TypeParameterCount + (InstanceTypeName?.TypeArgs.Length ?? 0)))
        {
            throw new LFCParserException("Type parameter count mismatch", Position);
        }

        Dictionary<LFCTypeToken, LFCTypeToken> typeMap;
        if (typeArgs == null)
        {
            typeMap = new Dictionary<LFCTypeToken, LFCTypeToken>();
        }
        else if (InstanceTypeName != null)
        {
            typeMap = LFCScope.CreateTypeMap(typeArgs, InstanceTypeName.Value.TypeArgs.Concat(TypeParameters).ToArray());
        }
        else
        {
            typeMap = LFCScope.CreateTypeMap(typeArgs, TypeParameters.ToArray());
        }

        LFCScope scope = new LFCScope(ReturnTypeName, typeMap, db, Position);

        foreach (LFCFunctionParameter parameter in Parameters)
        {
            scope.DefineParameter(parameter.Name, scope.GetType(parameter.TypeName, db, Position), Position);
        }

        return scope;
    }

    public override void Compile(LFReaderResult result)
    {
        if (HasTypeParameters)
        {
            //Logger.Debug($"Defering Compilation of {ToString()} until invocation");

            return;
        }

        Compile(result, Array.Empty<LFCType>());
    }
}