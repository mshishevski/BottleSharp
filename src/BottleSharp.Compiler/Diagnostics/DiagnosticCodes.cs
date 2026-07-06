namespace BottleSharp.Compiler.Diagnostics;

public static class DiagnosticCodes
{
    public const string UnexpectedCharacter = "BSHARP001";
    public const string UnterminatedString = "BSHARP002";
    public const string InvalidIntegerLiteral = "BSHARP003";

    public const string UnexpectedToken = "BSHARP101";
    public const string ExpectedExpression = "BSHARP102";
    public const string InvalidStatement = "BSHARP103";

    public const string UndefinedVariable = "BSHARP201";
    public const string DuplicateVariableDeclaration = "BSHARP202";
    public const string CannotAssign = "BSHARP203";
    public const string InvalidUnaryOperator = "BSHARP204";
    public const string InvalidBinaryOperator = "BSHARP205";
    public const string ConditionMustBeBool = "BSHARP206";

    public const string EmptySource = "BSHARP900";
}
