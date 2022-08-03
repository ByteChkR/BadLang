namespace LF.Compiler.C.Operators;

public abstract class LFCOperator
{
    protected LFCOperator(int precedence, string symbol)
    {
        Precedence = precedence;
        Symbol = symbol;
    }

    public int Precedence { get; }
    public string Symbol { get; }
}