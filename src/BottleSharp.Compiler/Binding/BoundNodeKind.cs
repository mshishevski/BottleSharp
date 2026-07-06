namespace BottleSharp.Compiler.Binding;

public enum BoundNodeKind
{
    BlockStatement,
    VariableDeclarationStatement,
    AssignmentStatement,
    PrintStatement,
    IfStatement,
    WhileStatement,

    LiteralExpression,
    VariableExpression,
    UnaryExpression,
    BinaryExpression,
    ErrorExpression,
}
