namespace BottleSharp.Compiler.Syntax;

public sealed class WhileStatementSyntax : StatementSyntax
{
    public WhileStatementSyntax(
        SyntaxToken whileKeyword,
        SyntaxToken openParenToken,
        ExpressionSyntax condition,
        SyntaxToken closeParenToken,
        BlockStatementSyntax body)
    {
        WhileKeyword = whileKeyword;
        OpenParenToken = openParenToken;
        Condition = condition;
        CloseParenToken = closeParenToken;
        Body = body;
    }

    public override SyntaxKind Kind => SyntaxKind.WhileStatement;

    public SyntaxToken WhileKeyword { get; }

    public SyntaxToken OpenParenToken { get; }

    public ExpressionSyntax Condition { get; }

    public SyntaxToken CloseParenToken { get; }

    public BlockStatementSyntax Body { get; }
}
