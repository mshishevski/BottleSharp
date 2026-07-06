using System.Collections.Immutable;
using BottleSharp.Compiler.Diagnostics;
using BottleSharp.Compiler.Syntax;
using BottleSharp.Compiler.Text;

namespace BottleSharp.Compiler.Binding;

public sealed class Binder
{
    private readonly SourceText _source;
    private readonly DiagnosticBag _diagnostics = new();

    private BoundScope _scope;

    public Binder(SourceText source)
    {
        _source = source;
        _scope = new BoundScope(parent: null);
    }

    public BindingResult Bind(CompilationUnitSyntax root)
    {
        var statements = ImmutableArray.CreateBuilder<BoundStatement>();
        foreach (var statement in root.Statements)
        {
            statements.Add(BindStatement(statement));
        }

        return new BindingResult(new BoundBlockStatement(statements.ToImmutable()), _diagnostics.ToImmutableArray());
    }

    private BoundStatement BindStatement(StatementSyntax syntax)
    {
        return syntax switch
        {
            BlockStatementSyntax block => BindBlockStatement(block),
            VariableDeclarationSyntax declaration => BindVariableDeclaration(declaration),
            AssignmentStatementSyntax assignment => BindAssignmentStatement(assignment),
            PrintStatementSyntax print => new BoundPrintStatement(BindExpression(print.Expression)),
            IfStatementSyntax ifStatement => BindIfStatement(ifStatement),
            WhileStatementSyntax whileStatement => BindWhileStatement(whileStatement),
            InvalidStatementSyntax => new BoundBlockStatement(ImmutableArray<BoundStatement>.Empty),
            _ => throw new InvalidOperationException($"Unexpected statement syntax '{syntax.Kind}'."),
        };
    }

    private BoundStatement BindBlockStatement(BlockStatementSyntax syntax)
    {
        var parent = _scope;
        _scope = new BoundScope(parent);

        var statements = ImmutableArray.CreateBuilder<BoundStatement>();
        foreach (var statement in syntax.Statements)
        {
            statements.Add(BindStatement(statement));
        }

        _scope = parent;
        return new BoundBlockStatement(statements.ToImmutable());
    }

    private BoundStatement BindVariableDeclaration(VariableDeclarationSyntax syntax)
    {
        var initializer = BindExpression(syntax.Initializer);
        var variable = new VariableSymbol(syntax.IdentifierToken.Text, initializer.Type);

        if (!_scope.TryDeclare(variable))
        {
            _diagnostics.Report(
                DiagnosticCodes.DuplicateVariableDeclaration,
                $"Variable '{variable.Name}' is already declared in this scope.",
                _source,
                CreateSpan(syntax.IdentifierToken));
        }

        return new BoundVariableDeclarationStatement(variable, initializer);
    }

    private BoundStatement BindAssignmentStatement(AssignmentStatementSyntax syntax)
    {
        var expression = BindExpression(syntax.Expression);

        if (!_scope.TryLookup(syntax.IdentifierToken.Text, out var variable) || variable is null)
        {
            _diagnostics.Report(
                DiagnosticCodes.UndefinedVariable,
                $"Variable '{syntax.IdentifierToken.Text}' is not declared.",
                _source,
                CreateSpan(syntax.IdentifierToken));

            var fallback = new VariableSymbol(syntax.IdentifierToken.Text, TypeSymbol.Error);
            return new BoundAssignmentStatement(fallback, expression);
        }

        if (expression.Type != TypeSymbol.Error && variable.Type != expression.Type)
        {
            _diagnostics.Report(
                DiagnosticCodes.CannotAssign,
                $"Cannot assign expression of type '{expression.Type}' to variable '{variable.Name}' of type '{variable.Type}'.",
                _source,
                CreateSpan(syntax.EqualsToken));
        }

        return new BoundAssignmentStatement(variable, expression);
    }

    private BoundStatement BindIfStatement(IfStatementSyntax syntax)
    {
        var condition = BindExpression(syntax.Condition);
        EnsureConditionIsBool(condition, syntax.OpenParenToken);

        var thenStatement = BindStatement(syntax.ThenBlock);
        var elseStatement = syntax.ElseClause is null ? null : BindStatement(syntax.ElseClause.ElseBlock);

        return new BoundIfStatement(condition, thenStatement, elseStatement);
    }

    private BoundStatement BindWhileStatement(WhileStatementSyntax syntax)
    {
        var condition = BindExpression(syntax.Condition);
        EnsureConditionIsBool(condition, syntax.OpenParenToken);

        var body = BindStatement(syntax.Body);
        return new BoundWhileStatement(condition, body);
    }

    private void EnsureConditionIsBool(BoundExpression condition, SyntaxToken token)
    {
        if (condition.Type == TypeSymbol.Error)
        {
            return;
        }

        if (condition.Type != TypeSymbol.Bool)
        {
            _diagnostics.Report(
                DiagnosticCodes.ConditionMustBeBool,
                $"Condition expression must be of type 'bool', but was '{condition.Type}'.",
                _source,
                CreateSpan(token));
        }
    }

    private BoundExpression BindExpression(ExpressionSyntax syntax)
    {
        return syntax switch
        {
            LiteralExpressionSyntax literal => BindLiteralExpression(literal),
            NameExpressionSyntax name => BindNameExpression(name),
            ParenthesizedExpressionSyntax parenthesized => BindExpression(parenthesized.Expression),
            UnaryExpressionSyntax unary => BindUnaryExpression(unary),
            BinaryExpressionSyntax binary => BindBinaryExpression(binary),
            _ => throw new InvalidOperationException($"Unexpected expression syntax '{syntax.Kind}'."),
        };
    }

    private BoundExpression BindLiteralExpression(LiteralExpressionSyntax syntax)
    {
        if (syntax.Value is int intValue)
        {
            return new BoundLiteralExpression(intValue, TypeSymbol.Int);
        }

        if (syntax.Value is string stringValue)
        {
            return new BoundLiteralExpression(stringValue, TypeSymbol.String);
        }

        if (syntax.Value is bool boolValue)
        {
            return new BoundLiteralExpression(boolValue, TypeSymbol.Bool);
        }

        return BoundErrorExpression.Instance;
    }

    private BoundExpression BindNameExpression(NameExpressionSyntax syntax)
    {
        if (string.IsNullOrWhiteSpace(syntax.IdentifierToken.Text))
        {
            return BoundErrorExpression.Instance;
        }

        if (!_scope.TryLookup(syntax.IdentifierToken.Text, out var variable) || variable is null)
        {
            _diagnostics.Report(
                DiagnosticCodes.UndefinedVariable,
                $"Variable '{syntax.IdentifierToken.Text}' is not declared.",
                _source,
                CreateSpan(syntax.IdentifierToken));

            return BoundErrorExpression.Instance;
        }

        return new BoundVariableExpression(variable);
    }

    private BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax)
    {
        var operand = BindExpression(syntax.Operand);
        if (operand.Type == TypeSymbol.Error)
        {
            return BoundErrorExpression.Instance;
        }

        var op = BoundUnaryOperator.Bind(syntax.OperatorToken.Kind, operand.Type);
        if (op is null)
        {
            _diagnostics.Report(
                DiagnosticCodes.InvalidUnaryOperator,
                $"Cannot apply unary operator '{syntax.OperatorToken.Text}' to operand of type '{operand.Type}'.",
                _source,
                CreateSpan(syntax.OperatorToken));

            return BoundErrorExpression.Instance;
        }

        return new BoundUnaryExpression(op, operand);
    }

    private BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax)
    {
        var left = BindExpression(syntax.Left);
        var right = BindExpression(syntax.Right);

        if (left.Type == TypeSymbol.Error || right.Type == TypeSymbol.Error)
        {
            return BoundErrorExpression.Instance;
        }

        var op = BoundBinaryOperator.Bind(syntax.OperatorToken.Kind, left.Type, right.Type);
        if (op is null)
        {
            _diagnostics.Report(
                DiagnosticCodes.InvalidBinaryOperator,
                $"Cannot apply operator '{syntax.OperatorToken.Text}' to operands of type '{left.Type}' and '{right.Type}'.",
                _source,
                CreateSpan(syntax.OperatorToken));

            return BoundErrorExpression.Instance;
        }

        return new BoundBinaryExpression(left, op, right);
    }

    private static TextSpan CreateSpan(SyntaxToken token)
    {
        return new TextSpan(token.Position, Math.Max(1, token.Text.Length));
    }
}
