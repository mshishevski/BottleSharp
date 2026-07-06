namespace BottleSharp.Compiler.Syntax;

public sealed class IfStatementSyntax : StatementSyntax
{
    public IfStatementSyntax(
        SyntaxToken ifKeyword,
        SyntaxToken openParenToken,
        ExpressionSyntax condition,
        SyntaxToken closeParenToken,
        BlockStatementSyntax thenBlock,
        ElseClauseSyntax? elseClause)
    {
        IfKeyword = ifKeyword;
        OpenParenToken = openParenToken;
        Condition = condition;
        CloseParenToken = closeParenToken;
        ThenBlock = thenBlock;
        ElseClause = elseClause;
    }

    public override SyntaxKind Kind => SyntaxKind.IfStatement;

    public SyntaxToken IfKeyword { get; }

    public SyntaxToken OpenParenToken { get; }

    public ExpressionSyntax Condition { get; }

    public SyntaxToken CloseParenToken { get; }

    public BlockStatementSyntax ThenBlock { get; }

    public ElseClauseSyntax? ElseClause { get; }
}
