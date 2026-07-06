namespace BottleSharp.Compiler.Syntax;

public sealed class ElseClauseSyntax : SyntaxNode
{
    public ElseClauseSyntax(SyntaxToken elseKeyword, BlockStatementSyntax elseBlock)
    {
        ElseKeyword = elseKeyword;
        ElseBlock = elseBlock;
    }

    public override SyntaxKind Kind => SyntaxKind.ElseClause;

    public SyntaxToken ElseKeyword { get; }

    public BlockStatementSyntax ElseBlock { get; }
}
