namespace BottleSharp.Compiler.Syntax;

public sealed class InvalidStatementSyntax : StatementSyntax
{
    public InvalidStatementSyntax(SyntaxToken token)
    {
        Token = token;
    }

    public override SyntaxKind Kind => SyntaxKind.InvalidStatement;

    public SyntaxToken Token { get; }
}
