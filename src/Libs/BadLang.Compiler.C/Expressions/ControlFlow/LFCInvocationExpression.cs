using LF.Compiler.C.Functions;
using LF.Compiler.C.Types;
using LF.Compiler.Reader;

namespace LF.Compiler.C.Expressions.ControlFlow;

public class LFCInvocationExpression : LFCExpression
{
    public readonly LFCExpression Left;
    private readonly LFCExpression[] m_Parameters;
    private readonly LFCTypeToken[] m_TypeArgs;

    public LFCInvocationExpression(LFSourcePosition position, LFCExpression left, LFCExpression[] parameters, LFCTypeToken[] typeArgs) : base(position)
    {
        Left = left;
        m_Parameters = parameters;
        m_TypeArgs = typeArgs;
    }

    public IEnumerable<LFCExpression> Parameters => m_Parameters;

    public override LFCExpression Rebuild(Func<LFCExpression, LFCExpression> rebuilder)
    {
        LFCExpression[] parameters = new LFCExpression[m_Parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            parameters[i] = rebuilder(m_Parameters[i]);
        }

        return new LFCInvocationExpression(Position, rebuilder(Left), parameters, m_TypeArgs);
    }


    public static void CheckInstanceTypes(
        LFCFunctionType funcType,
        LFCTypeDatabase db,
        LFCExpressionCompileResult[] parameterResults,
        LFCType[] typeArgs,
        LFCType? typeHint,
        LFSourcePosition position)
    {
        Dictionary<LFCTypeToken, LFCTypeToken> typeMap = LFCScope.CreateTypeMap(
            typeArgs,
            funcType.SourceFunction!.InstanceTypeName!.Value.TypeArgs.Concat(funcType.TypeParameters).ToArray()
        );


        if (typeHint != null && LFCScope.GetType(funcType.ReturnTypeName, db, typeMap, position) != typeHint)
        {
            throw new LFCParserException("Function return type does not match type hint", position);
        }

        if (funcType.ParameterCount - 1 != parameterResults.Length)
        {
            throw new LFCParserException("Function parameter count does not match parameter count", position);
        }

        if (funcType.HasTypeParameters)
        {
            if (funcType.TypeParameterCount != typeArgs.Length)
            {
                throw new LFCParserException("Function type parameter count does not match type argument count", position);
            }
        }

        for (int i = 0; i < parameterResults.Length; i++)
        {
            LFCExpressionCompileResult pResult = parameterResults[i];
            LFCType pType = LFCScope.GetType(funcType.GetParameterType(i + 1), db, typeMap, position);
            if (pType != pResult.ReturnType)
            {
                throw new LFCParserException("Invocation argument type does not match function parameter type", position);
            }
        }
    }

    private static void CheckTypes(
        LFCFunctionType funcType,
        LFCTypeDatabase db,
        LFCExpressionCompileResult[] parameterResults,
        LFCType[] typeArgs,
        LFCType? typeHint,
        LFSourcePosition position)
    {
        Dictionary<LFCTypeToken, LFCTypeToken> typeMap = LFCScope.CreateTypeMap(
            typeArgs,
            funcType.TypeParameters.ToArray()
        );


        if (typeHint != null && LFCScope.GetType(funcType.ReturnTypeName, db, typeMap, position) != typeHint)
        {
            throw new LFCParserException("Function return type does not match type hint", position);
        }

        if (funcType.ParameterCount != parameterResults.Length)
        {
            throw new LFCParserException("Function parameter count does not match parameter count", position);
        }

        if (funcType.HasTypeParameters)
        {
            if (funcType.TypeParameterCount != typeArgs.Length)
            {
                throw new LFCParserException("Function type parameter count does not match type argument count", position);
            }
        }

        for (int i = 0; i < parameterResults.Length; i++)
        {
            LFCExpressionCompileResult pResult = parameterResults[i];
            LFCType pType = LFCScope.GetType(funcType.GetParameterType(i), db, typeMap, position);
            if (pType != pResult.ReturnType)
            {
                throw new LFCParserException("Invocation argument type does not match function parameter type", position);
            }
        }
    }

    public LFCExpressionCompileResult InvokeInstance(LFCExpressionCompileInput input, LFCExpressionCompileResult leftResult, LFCTypeDatabase db, LFCFunctionType funcType)
    {
        List<LFCTypeToken> inheritedTypeArgs = new List<LFCTypeToken>(leftResult.InheritedTypeArgs);
        inheritedTypeArgs.AddRange(m_TypeArgs);
        LFCExpressionCompileResult[] parameterResults = new LFCExpressionCompileResult[m_Parameters.Length];

        //parameterResults[0] = leftResult;
        for (int i = 0; i < m_Parameters.Length; i++)
        {
            LFCType pType;
            if (inheritedTypeArgs.Count != 0)
            {
                Dictionary<LFCTypeToken, LFCTypeToken> map = LFCScope.CreateTypeMap(
                    inheritedTypeArgs.Select(x => input.GetType(x, Position)).ToArray(),
                    funcType.SourceFunction!.InstanceTypeName!.Value.TypeArgs.Concat(funcType.TypeParameters).ToArray()
                );
                pType = LFCScope.GetType(funcType.GetParameterType(i + 1), db, map, Position);
            }
            else
            {
                pType = db.GetType(funcType.GetParameterType(i + 1), Position);
            }

            parameterResults[i] = m_Parameters[i]
                .Compile(new LFCExpressionCompileInput(input.Result, input.Section, input.Scope, false, pType));
            if (!parameterResults[i].EmittedAny)
            {
                if (parameterResults[i].ReturnType is LFCFunctionType pFuncType)
                {
                    if (pFuncType.SourceFunction == null)
                    {
                        throw new LFCParserException("Source function is null", Position);
                    }

                    string pFuncName;
                    if (pFuncType.HasTypeParameters)
                    {
                        //Enqueue the compilation of the function with the resolved arguments
                        LFCType[] typeArgs = inheritedTypeArgs.Select(x => input.GetType(x, Position)).ToArray();
                        pFuncName = LFCFunction.GetFunctionLabel(pFuncType.SourceFunction.GetFullName(input.Section), typeArgs);
                        if (!input.Result.HasLabel(pFuncName, input.Section))
                        {
                            input.Result.OnFinalize(() => pFuncType.SourceFunction.Compile(input.Result, typeArgs));
                        }

                        // else
                        // {
                        //     Logger.Debug($"Skipping Compilation of Function {pFuncName}");
                        // }
                    }
                    else
                    {
                        pFuncName = LFCFunction.GetFunctionLabel(pFuncType.SourceFunction.GetFullName(input.Section), Array.Empty<LFCType>());
                    }

                    input.Section.Emit(Position, OpCodes.Push64, pFuncName);
                }
                else
                {
                    throw new LFCParserException("Left expression must be a function", Position);
                }
            }
        }

        CheckInstanceTypes(funcType, db, parameterResults, inheritedTypeArgs.Select(x => input.GetType(x, Position)).ToArray(), input.TypeHint, Position);
        if (funcType.SourceFunction == null)
        {
            throw new LFCParserException("Function type has no source function", Position);
        }

        string funcName;
        if (inheritedTypeArgs.Count != 0)
        {
            //Enqueue the compilation of the function with the resolved arguments
            LFCType[] typeArgs = inheritedTypeArgs.Select(x => input.GetType(x, Position)).ToArray();
            funcName = LFCFunction.GetFunctionLabel(funcType.SourceFunction.GetFullName(input.Section), typeArgs);
            if (!input.Result.HasLabel(funcName, input.Section))
            {
                input.Result.OnFinalize(() => funcType.SourceFunction.Compile(input.Result, typeArgs));
            }

            // else
            // {
            //     Logger.Debug($"Skipping Compilation of Function {funcName}");
            // }
        }
        else
        {
            funcName = LFCFunction.GetFunctionLabel(funcType.SourceFunction.GetFullName(input.Section), Array.Empty<LFCType>());
        }


        input.Section.Emit(Position, OpCodes.Push64, funcName);

        input.Section.Emit(Position, OpCodes.Call);

        return input.CreateResult(
            LFCScope.GetType(
                funcType.ReturnTypeName,
                db,
                LFCScope.CreateTypeMap(
                    inheritedTypeArgs.Select(x => input.GetType(x, Position)).ToArray(),
                    funcType.SourceFunction!.InstanceTypeName!.Value.TypeArgs.Concat(funcType.TypeParameters).ToArray()
                ),
                Position
            ),
            Position
        );
    }

    public override LFCExpressionCompileResult Compile(LFCExpressionCompileInput input)
    {
        LFCTypeDatabase db = LFCTypeDatabase.GetTypeSystem(input.Result);
        LFCExpressionCompileResult leftResult =
            Left.Compile(new LFCExpressionCompileInput(input.Result, input.Section, input.Scope, false));

        if (leftResult.ReturnType is LFCFunctionType function && function.IsInstance)
        {
            return InvokeInstance(input, leftResult, db, function);
        }

        bool emittedAny = leftResult.EmittedAny;
        if (leftResult.EmittedAny)
        {
            leftResult.RemoveEmitted();
        }

        if (leftResult.ReturnType is not LFCFunctionType funcType)
        {
            throw new LFCParserException("Left expression must be a function", Position);
        }

        LFCExpressionCompileResult[] parameterResults = new LFCExpressionCompileResult[m_Parameters.Length];
        for (int i = 0; i < m_Parameters.Length; i++)
        {
            LFCType pType;
            if (m_TypeArgs.Length != 0)
            {
                Dictionary<LFCTypeToken, LFCTypeToken> map = LFCScope.CreateTypeMap(m_TypeArgs.Select(x => input.GetType(x, Position)).ToArray(), funcType.TypeParameters.ToArray());
                pType = LFCScope.GetType(funcType.GetParameterType(i), db, map, Position);
            }
            else
            {
                pType = db.GetType(funcType.GetParameterType(i), Position);
            }

            parameterResults[i] = m_Parameters[i]
                .Compile(new LFCExpressionCompileInput(input.Result, input.Section, input.Scope, false, pType));
            if (!parameterResults[i].EmittedAny)
            {
                if (parameterResults[i].ReturnType is LFCFunctionType pFuncType)
                {
                    if (pFuncType.SourceFunction == null)
                    {
                        throw new LFCParserException("Source function is null", Position);
                    }

                    string pFuncName;
                    if (pFuncType.HasTypeParameters)
                    {
                        //Enqueue the compilation of the function with the resolved arguments
                        LFCType[] typeArgs = m_TypeArgs.Select(x => input.GetType(x, Position)).ToArray();
                        pFuncName = LFCFunction.GetFunctionLabel(pFuncType.SourceFunction.GetFullName(input.Section), typeArgs);
                        if (!input.Result.HasLabel(pFuncName, input.Section))
                        {
                            input.Result.OnFinalize(() => pFuncType.SourceFunction.Compile(input.Result, typeArgs));
                        }

                        // else
                        // {
                        //     Logger.Debug($"Skipping Compilation of Function {pFuncName}");
                        // }
                    }
                    else
                    {
                        pFuncName = LFCFunction.GetFunctionLabel(pFuncType.SourceFunction.GetFullName(input.Section), Array.Empty<LFCType>());
                    }

                    input.Section.Emit(m_Parameters[i].Position, OpCodes.Push64, pFuncName);
                }
                else
                {
                    throw new LFCParserException("Left expression must be a function", Position);
                }
            }
        }


        CheckTypes(funcType, db, parameterResults, m_TypeArgs.Select(x => input.GetType(x, Position)).ToArray(), input.TypeHint, Position);


        if (emittedAny)
        {
            leftResult =
                Left.Compile(new LFCExpressionCompileInput(input.Result, input.Section, input.Scope, false));
        }
        else
        {
            if (funcType.SourceFunction == null)
            {
                throw new LFCParserException("Function type has no source function", Position);
            }

            string funcName;
            if (funcType.HasTypeParameters)
            {
                //Enqueue the compilation of the function with the resolved arguments
                LFCType[] typeArgs = m_TypeArgs.Select(x => input.GetType(x, Position)).ToArray();
                funcName = LFCFunction.GetFunctionLabel(funcType.SourceFunction.GetFullName(input.Section), typeArgs);
                if (!input.Result.HasLabel(funcName, input.Section))
                {
                    input.Result.OnFinalize(() => funcType.SourceFunction.Compile(input.Result, typeArgs));
                }

                // else
                // {
                //     Logger.Debug($"Skipping Compilation of Function {funcName}");
                // }
            }
            else
            {
                funcName = LFCFunction.GetFunctionLabel(funcType.SourceFunction.GetFullName(input.Section), Array.Empty<LFCType>());
            }


            input.Section.Emit(Position, OpCodes.Push64, funcName);
        }

        input.Section.Emit(Position, OpCodes.Call);

        return input.CreateResult(
            LFCScope.GetType(
                funcType.ReturnTypeName,
                db,
                LFCScope.CreateTypeMap(
                    m_TypeArgs.Select(x => input.GetType(x, Position)).ToArray(),
                    funcType.TypeParameters.ToArray()
                ),
                Position
            ),
            Position
        );
    }
}