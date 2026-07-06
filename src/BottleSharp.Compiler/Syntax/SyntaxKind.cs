namespace BottleSharp.Compiler.Syntax;

public enum SyntaxKind
{
    CompilationUnit,

    BlockStatement,
    VariableDeclarationStatement,
    AssignmentStatement,
    PrintStatement,
    IfStatement,
    ElseClause,
    WhileStatement,
    InvalidStatement,

    LiteralExpression,
    NameExpression,
    ParenthesizedExpression,
    UnaryExpression,
    BinaryExpression,

    BadToken,
    EndOfFileToken,

    IdentifierToken,
    IntLiteralToken,
    StringLiteralToken,

    LetKeyword,
    PrintKeyword,
    IfKeyword,
    ElseKeyword,
    WhileKeyword,
    TrueKeyword,
    FalseKeyword,

    PlusToken,
    MinusToken,
    StarToken,
    SlashToken,
    BangToken,

    EqualsToken,
    EqualsEqualsToken,
    BangEqualsToken,

    LessToken,
    LessOrEqualsToken,
    GreaterToken,
    GreaterOrEqualsToken,

    AmpersandAmpersandToken,
    PipePipeToken,

    OpenParenToken,
    CloseParenToken,
    OpenBraceToken,
    CloseBraceToken,
    SemicolonToken,
    CommaToken,
}
