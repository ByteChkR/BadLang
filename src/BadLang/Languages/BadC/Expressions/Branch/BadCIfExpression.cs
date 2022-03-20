using BadC.DebugSymbols;
using BadC.Expressions.Values.Symbols;
using BadC.Templates;
using BadC.Types;
using BadC.Types.Primitives;
using BadC.Utils;

using BadVM.Shared;
using BadVM.Shared.AssemblyFormat;

namespace BadC.Expressions.Branch;

public class BadCIfExpression : BadCExpression
{

    public Dictionary < BadCExpression, BadCExpression[] > ConditionBodies { get; }

    public BadCExpression[] ElseBody { get; }

    #region Public

    public BadCIfExpression(
        Dictionary < BadCExpression, BadCExpression[] > conditionBodies,
        BadCExpression[] elseBody,
        SourceToken token ) : base( false, token )
    {
        ConditionBodies = conditionBodies;
        ElseBody = elseBody;
    }

    public override void Emit(
        BadCEmitContext context,
        BadCType typeHint,
        bool isLval )
    {
        AssemblySymbol endLabel = context.Writer.GetUniqueName( "IF_END" );

        context.AddSymbol( context.Writer, SourceToken );

        foreach ( KeyValuePair < BadCExpression, BadCExpression[] > condition in ConditionBodies )
        {
            if ( condition.Value.Length == 0 )
            {
                continue;
            }

            BadCType? baseTypeHint = condition.Key.GetFixedType( context );

            if ( baseTypeHint == null )
            {
                baseTypeHint = BadCType.GetPrimitive( BadCPrimitiveTypes.I64 );
            }

            AssemblySymbol jmpLabel = context.Writer.GetUniqueName( "IF_COND" );
            condition.Key.Emit( context, baseTypeHint, false );

            context.Writer.BranchZero( baseTypeHint, jmpLabel );

            foreach ( BadCExpression expr in condition.Value ) //Emit Body of branch
            {
                if ( expr is BadCVariableDeclaration decl )
                {
                    expr.Emit( context, decl.Type, false );
                }
                else
                {
                    expr.Emit(
                              context,
                              BadCType.GetPrimitive( BadCPrimitiveTypes.Void ),
                              false
                             );
                }
            }

            //We executed the branch. No other branches will be executed.
            context.Writer.Emit( OpCode.PushI64, BitConverter.GetBytes( ( long )0 ) );
            context.Writer.AddPatchSite( endLabel, context.Writer.CurrentSize - sizeof( long ), sizeof( long ) );
            context.Writer.Emit( OpCode.Jump, Array.Empty < byte >() );

            //Set Label if branch is false
            context.Writer.Label( jmpLabel, AssemblyElementVisibility.Local );
        }

        //All branches failed. Execute Else Body
        if ( ElseBody.Length != 0 )
        {
            foreach ( BadCExpression expr in ElseBody ) //Emit Body of branch
            {
                if ( expr is BadCVariableDeclaration decl )
                {
                    expr.Emit( context, decl.Type, false );
                }
                else
                {
                    expr.Emit(
                              context,
                              BadCType.GetPrimitive( BadCPrimitiveTypes.Void ),
                              false
                             );
                }
            }
        }

        context.Writer.Label( endLabel, AssemblyElementVisibility.Local );
    }

    public override BadCType? GetFixedType( BadCEmitContext context )
    {
        return BadCType.GetPrimitive( BadCPrimitiveTypes.Void );
    }

    public override BadCExpression ResolveTemplateTypes( BadCTemplateTypeContext templateContext )
    {
        Dictionary < BadCExpression, BadCExpression[] > conditionBodies =
            new Dictionary < BadCExpression, BadCExpression[] >();

        foreach ( KeyValuePair < BadCExpression, BadCExpression[] > condition in ConditionBodies )
        {
            BadCExpression[] newBody = new BadCExpression[condition.Value.Length];

            for ( int i = 0; i < condition.Value.Length; i++ )
            {
                newBody[i] = condition.Value[i].ResolveTemplateTypes( templateContext );
            }

            conditionBodies.Add(
                                condition.Key.ResolveTemplateTypes( templateContext ),
                                newBody
                               );
        }

        BadCExpression[] elseBody = new BadCExpression[ElseBody.Length];

        for ( int i = 0; i < ElseBody.Length; i++ )
        {
            elseBody[i] = ElseBody[i].ResolveTemplateTypes( templateContext );
        }

        return new BadCIfExpression( conditionBodies, elseBody, SourceToken );
    }

    #endregion

}
