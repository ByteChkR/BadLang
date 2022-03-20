using BadC.DebugSymbols;
using BadC.Expressions.Values.Symbols;
using BadC.Templates;
using BadC.Types;
using BadC.Types.Primitives;
using BadC.Utils;

using BadVM.Shared;
using BadVM.Shared.AssemblyFormat;

namespace BadC.Expressions.Branch;

public class BadCWhileExpression : BadCExpression
{

    public BadCExpression Condition { get; }

    public BadCExpression[] Body { get; }

    #region Public

    public BadCWhileExpression( BadCExpression condition, BadCExpression[] body, SourceToken token ) :
        base( false, token )
    {
        Condition = condition;
        Body = body;
    }

    public override void Emit(
        BadCEmitContext context,
        BadCType typeHint,
        bool isLval )
    {
        AssemblySymbol whileStart = context.Writer.GetUniqueName( "WHILE_START" );
        AssemblySymbol whileEnd = context.Writer.GetUniqueName( "WHILE_END" );
        BadCType? baseTypeHint = Condition.GetFixedType( context );

        if ( baseTypeHint == null )
        {
            baseTypeHint = BadCType.GetPrimitive( BadCPrimitiveTypes.I64 );
        }

        context.AddSymbol( context.Writer, SourceToken );
        context.Writer.Label( whileStart, AssemblyElementVisibility.Local );
        Condition.Emit( context, baseTypeHint, false );

        //Check if condition is false. If so, jump to end.
        context.Writer.BranchZero( baseTypeHint, whileEnd );

        foreach ( BadCExpression expr in Body ) //Emit Body of branch
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

        //Jump Back to top
        context.Writer.Jump( whileStart );

        context.Writer.Label( whileEnd, AssemblyElementVisibility.Local );
    }

    public override BadCType? GetFixedType( BadCEmitContext context )
    {
        return BadCType.GetPrimitive( BadCPrimitiveTypes.Void );
    }

    public override BadCExpression ResolveTemplateTypes( BadCTemplateTypeContext templateContext )
    {
        BadCExpression[] newBody = new BadCExpression[Body.Length];

        for ( int i = 0; i < Body.Length; i++ )
        {
            newBody[i] = Body[i].ResolveTemplateTypes( templateContext );
        }

        return new BadCWhileExpression(
                                       Condition.ResolveTemplateTypes( templateContext ),
                                       newBody,
                                       SourceToken
                                      );
    }

    #endregion

}
