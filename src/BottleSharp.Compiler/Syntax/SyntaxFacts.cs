namespace BottleSharp.Compiler.Syntax;

public static class SyntaxFacts
{
    public static SyntaxKind GetKeywordKind(string text)
    {
        return text switch
        {
            "let" => SyntaxKind.LetKeyword,
            "print" => SyntaxKind.PrintKeyword,
            "if" => SyntaxKind.IfKeyword,
            "else" => SyntaxKind.ElseKeyword,
            "while" => SyntaxKind.WhileKeyword,
            "true" => SyntaxKind.TrueKeyword,
            "false" => SyntaxKind.FalseKeyword,
            _ => SyntaxKind.IdentifierToken,
        };
    }

    public static int GetUnaryOperatorPrecedence(SyntaxKind kind)
    {
        return kind switch
        {
            SyntaxKind.BangToken => 7,
            SyntaxKind.MinusToken => 7,
            _ => 0,
        };
    }

    public static int GetBinaryOperatorPrecedence(SyntaxKind kind)
    {
        return kind switch
        {
            SyntaxKind.StarToken => 6,
            SyntaxKind.SlashToken => 6,

            SyntaxKind.PlusToken => 5,
            SyntaxKind.MinusToken => 5,

            SyntaxKind.LessToken => 4,
            SyntaxKind.LessOrEqualsToken => 4,
            SyntaxKind.GreaterToken => 4,
            SyntaxKind.GreaterOrEqualsToken => 4,

            SyntaxKind.EqualsEqualsToken => 3,
            SyntaxKind.BangEqualsToken => 3,

            SyntaxKind.AmpersandAmpersandToken => 2,
            SyntaxKind.PipePipeToken => 1,
            _ => 0,
        };
    }
}
