using BottleSharp.Compiler.Syntax;

namespace BottleSharp.Compiler.Binding;

public sealed class BoundBinaryOperator
{
    private BoundBinaryOperator(
        SyntaxKind syntaxKind,
        TypeSymbol leftType,
        TypeSymbol rightType,
        TypeSymbol resultType)
    {
        SyntaxKind = syntaxKind;
        LeftType = leftType;
        RightType = rightType;
        ResultType = resultType;
    }

    public SyntaxKind SyntaxKind { get; }

    public TypeSymbol LeftType { get; }

    public TypeSymbol RightType { get; }

    public TypeSymbol ResultType { get; }

    public static BoundBinaryOperator? Bind(SyntaxKind syntaxKind, TypeSymbol leftType, TypeSymbol rightType)
    {
        return Operators.SingleOrDefault(op =>
            op.SyntaxKind == syntaxKind &&
            op.LeftType == leftType &&
            op.RightType == rightType);
    }

    private static IReadOnlyList<BoundBinaryOperator> Operators { get; } =
    [
        new BoundBinaryOperator(SyntaxKind.PlusToken, TypeSymbol.Int, TypeSymbol.Int, TypeSymbol.Int),
        new BoundBinaryOperator(SyntaxKind.MinusToken, TypeSymbol.Int, TypeSymbol.Int, TypeSymbol.Int),
        new BoundBinaryOperator(SyntaxKind.StarToken, TypeSymbol.Int, TypeSymbol.Int, TypeSymbol.Int),
        new BoundBinaryOperator(SyntaxKind.SlashToken, TypeSymbol.Int, TypeSymbol.Int, TypeSymbol.Int),

        new BoundBinaryOperator(SyntaxKind.PlusToken, TypeSymbol.String, TypeSymbol.String, TypeSymbol.String),

        new BoundBinaryOperator(SyntaxKind.EqualsEqualsToken, TypeSymbol.Int, TypeSymbol.Int, TypeSymbol.Bool),
        new BoundBinaryOperator(SyntaxKind.BangEqualsToken, TypeSymbol.Int, TypeSymbol.Int, TypeSymbol.Bool),
        new BoundBinaryOperator(SyntaxKind.LessToken, TypeSymbol.Int, TypeSymbol.Int, TypeSymbol.Bool),
        new BoundBinaryOperator(SyntaxKind.LessOrEqualsToken, TypeSymbol.Int, TypeSymbol.Int, TypeSymbol.Bool),
        new BoundBinaryOperator(SyntaxKind.GreaterToken, TypeSymbol.Int, TypeSymbol.Int, TypeSymbol.Bool),
        new BoundBinaryOperator(SyntaxKind.GreaterOrEqualsToken, TypeSymbol.Int, TypeSymbol.Int, TypeSymbol.Bool),

        new BoundBinaryOperator(SyntaxKind.EqualsEqualsToken, TypeSymbol.String, TypeSymbol.String, TypeSymbol.Bool),
        new BoundBinaryOperator(SyntaxKind.BangEqualsToken, TypeSymbol.String, TypeSymbol.String, TypeSymbol.Bool),

        new BoundBinaryOperator(SyntaxKind.EqualsEqualsToken, TypeSymbol.Bool, TypeSymbol.Bool, TypeSymbol.Bool),
        new BoundBinaryOperator(SyntaxKind.BangEqualsToken, TypeSymbol.Bool, TypeSymbol.Bool, TypeSymbol.Bool),
        new BoundBinaryOperator(SyntaxKind.AmpersandAmpersandToken, TypeSymbol.Bool, TypeSymbol.Bool, TypeSymbol.Bool),
        new BoundBinaryOperator(SyntaxKind.PipePipeToken, TypeSymbol.Bool, TypeSymbol.Bool, TypeSymbol.Bool),
    ];
}
