namespace BottleSharp.Compiler.Syntax;

public sealed class VariableDeclarationSyntax : StatementSyntax
{
    public VariableDeclarationSyntax(
        SyntaxToken letKeyword,
        SyntaxToken identifierToken,
        SyntaxToken equalsToken,
        ExpressionSyntax initializer,
        SyntaxToken semicolonToken)
    {
        LetKeyword = letKeyword;
        IdentifierToken = identifierToken;
        EqualsToken = equalsToken;
        Initializer = initializer;
        SemicolonToken = semicolonToken;
    }

    public override SyntaxKind Kind => SyntaxKind.VariableDeclarationStatement;

    public SyntaxToken LetKeyword { get; }

    public SyntaxToken IdentifierToken { get; }

    public SyntaxToken EqualsToken { get; }

    public ExpressionSyntax Initializer { get; }

    public SyntaxToken SemicolonToken { get; }
}
