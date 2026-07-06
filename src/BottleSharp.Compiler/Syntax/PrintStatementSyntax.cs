namespace BottleSharp.Compiler.Syntax;

public sealed class PrintStatementSyntax : StatementSyntax
{
    public PrintStatementSyntax(
        SyntaxToken printKeyword,
        SyntaxToken openParenToken,
        ExpressionSyntax expression,
        SyntaxToken closeParenToken,
        SyntaxToken semicolonToken)
    {
        PrintKeyword = printKeyword;
        OpenParenToken = openParenToken;
        Expression = expression;
        CloseParenToken = closeParenToken;
        SemicolonToken = semicolonToken;
    }

    public override SyntaxKind Kind => SyntaxKind.PrintStatement;

    public SyntaxToken PrintKeyword { get; }

    public SyntaxToken OpenParenToken { get; }

    public ExpressionSyntax Expression { get; }

    public SyntaxToken CloseParenToken { get; }

    public SyntaxToken SemicolonToken { get; }
}
