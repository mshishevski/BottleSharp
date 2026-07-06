namespace BottleSharp.Compiler.Syntax;

public sealed class AssignmentStatementSyntax : StatementSyntax
{
    public AssignmentStatementSyntax(
        SyntaxToken identifierToken,
        SyntaxToken equalsToken,
        ExpressionSyntax expression,
        SyntaxToken semicolonToken)
    {
        IdentifierToken = identifierToken;
        EqualsToken = equalsToken;
        Expression = expression;
        SemicolonToken = semicolonToken;
    }

    public override SyntaxKind Kind => SyntaxKind.AssignmentStatement;

    public SyntaxToken IdentifierToken { get; }

    public SyntaxToken EqualsToken { get; }

    public ExpressionSyntax Expression { get; }

    public SyntaxToken SemicolonToken { get; }
}
