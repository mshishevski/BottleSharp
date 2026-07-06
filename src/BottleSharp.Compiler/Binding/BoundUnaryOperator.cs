using BottleSharp.Compiler.Syntax;

namespace BottleSharp.Compiler.Binding;

public sealed class BoundUnaryOperator
{
    private BoundUnaryOperator(SyntaxKind syntaxKind, TypeSymbol operandType, TypeSymbol resultType)
    {
        SyntaxKind = syntaxKind;
        OperandType = operandType;
        ResultType = resultType;
    }

    public SyntaxKind SyntaxKind { get; }

    public TypeSymbol OperandType { get; }

    public TypeSymbol ResultType { get; }

    public static BoundUnaryOperator? Bind(SyntaxKind syntaxKind, TypeSymbol operandType)
    {
        return Operators.SingleOrDefault(op => op.SyntaxKind == syntaxKind && op.OperandType == operandType);
    }

    private static IReadOnlyList<BoundUnaryOperator> Operators { get; } =
    [
        new BoundUnaryOperator(SyntaxKind.BangToken, TypeSymbol.Bool, TypeSymbol.Bool),
        new BoundUnaryOperator(SyntaxKind.MinusToken, TypeSymbol.Int, TypeSymbol.Int),
    ];
}
