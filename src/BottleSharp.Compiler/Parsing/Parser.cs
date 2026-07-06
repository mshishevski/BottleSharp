using System.Collections.Immutable;
using BottleSharp.Compiler.Diagnostics;
using BottleSharp.Compiler.Lexing;
using BottleSharp.Compiler.Syntax;
using BottleSharp.Compiler.Text;

namespace BottleSharp.Compiler.Parsing;

public sealed class Parser
{
    private readonly SourceText _source;
    private readonly SyntaxToken[] _tokens;
    private readonly DiagnosticBag _diagnostics = new();

    private int _position;

    public Parser(SourceText source, ImmutableArray<SyntaxToken> tokens)
    {
        _source = source;
        _tokens = tokens.Where(t => t.Kind != SyntaxKind.BadToken).ToArray();
    }

    public ParseResult ParseCompilationUnit()
    {
        var statements = ImmutableArray.CreateBuilder<StatementSyntax>();

        while (Current.Kind != SyntaxKind.EndOfFileToken)
        {
            var startToken = Current;
            var statement = ParseStatement();
            statements.Add(statement);

            if (Current == startToken)
            {
                NextToken();
            }
        }

        var eof = MatchToken(SyntaxKind.EndOfFileToken);
        var root = new CompilationUnitSyntax(statements.ToImmutable(), eof);
        return new ParseResult(root, _diagnostics.ToImmutableArray());
    }

    private SyntaxToken Current => Peek(0);

    private SyntaxToken Peek(int offset)
    {
        var index = _position + offset;
        if (index >= _tokens.Length)
        {
            return _tokens[^1];
        }

        return _tokens[index];
    }

    private SyntaxToken NextToken()
    {
        var current = Current;
        _position++;
        return current;
    }

    private SyntaxToken MatchToken(SyntaxKind kind)
    {
        if (Current.Kind == kind)
        {
            return NextToken();
        }

        var span = new TextSpan(Current.Position, Math.Max(1, Current.Text.Length));
        _diagnostics.Report(
            DiagnosticCodes.UnexpectedToken,
            $"Expected token '{kind}' but found '{Current.Kind}'.",
            _source,
            span);

        return new SyntaxToken(kind, Current.Position, string.Empty, null);
    }

    private StatementSyntax ParseStatement()
    {
        return Current.Kind switch
        {
            SyntaxKind.OpenBraceToken => ParseBlockStatement(),
            SyntaxKind.LetKeyword => ParseVariableDeclarationStatement(),
            SyntaxKind.PrintKeyword => ParsePrintStatement(),
            SyntaxKind.IfKeyword => ParseIfStatement(),
            SyntaxKind.WhileKeyword => ParseWhileStatement(),
            SyntaxKind.IdentifierToken when Peek(1).Kind == SyntaxKind.EqualsToken => ParseAssignmentStatement(),
            _ => ParseInvalidStatement(),
        };
    }

    private StatementSyntax ParseInvalidStatement()
    {
        var token = NextToken();
        var span = new TextSpan(token.Position, Math.Max(1, token.Text.Length));
        _diagnostics.Report(
            DiagnosticCodes.InvalidStatement,
            $"Invalid start of statement '{token.Kind}'.",
            _source,
            span);

        return new InvalidStatementSyntax(token);
    }

    private BlockStatementSyntax ParseBlockStatement()
    {
        var openBrace = MatchToken(SyntaxKind.OpenBraceToken);
        var statements = ImmutableArray.CreateBuilder<StatementSyntax>();

        while (Current.Kind != SyntaxKind.EndOfFileToken && Current.Kind != SyntaxKind.CloseBraceToken)
        {
            var startToken = Current;
            var statement = ParseStatement();
            statements.Add(statement);

            if (Current == startToken)
            {
                NextToken();
            }
        }

        var closeBrace = MatchToken(SyntaxKind.CloseBraceToken);
        return new BlockStatementSyntax(openBrace, statements.ToImmutable(), closeBrace);
    }

    private StatementSyntax ParseVariableDeclarationStatement()
    {
        var letKeyword = MatchToken(SyntaxKind.LetKeyword);
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        var equals = MatchToken(SyntaxKind.EqualsToken);
        var initializer = ParseExpression();
        var semicolon = MatchToken(SyntaxKind.SemicolonToken);

        return new VariableDeclarationSyntax(letKeyword, identifier, equals, initializer, semicolon);
    }

    private StatementSyntax ParseAssignmentStatement()
    {
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        var equals = MatchToken(SyntaxKind.EqualsToken);
        var expression = ParseExpression();
        var semicolon = MatchToken(SyntaxKind.SemicolonToken);

        return new AssignmentStatementSyntax(identifier, equals, expression, semicolon);
    }

    private StatementSyntax ParsePrintStatement()
    {
        var printKeyword = MatchToken(SyntaxKind.PrintKeyword);
        var openParen = MatchToken(SyntaxKind.OpenParenToken);
        var expression = ParseExpression();
        var closeParen = MatchToken(SyntaxKind.CloseParenToken);
        var semicolon = MatchToken(SyntaxKind.SemicolonToken);

        return new PrintStatementSyntax(printKeyword, openParen, expression, closeParen, semicolon);
    }

    private StatementSyntax ParseIfStatement()
    {
        var ifKeyword = MatchToken(SyntaxKind.IfKeyword);
        var openParen = MatchToken(SyntaxKind.OpenParenToken);
        var condition = ParseExpression();
        var closeParen = MatchToken(SyntaxKind.CloseParenToken);
        var thenBlock = ParseBlockStatement();

        ElseClauseSyntax? elseClause = null;
        if (Current.Kind == SyntaxKind.ElseKeyword)
        {
            var elseKeyword = MatchToken(SyntaxKind.ElseKeyword);
            var elseBlock = ParseBlockStatement();
            elseClause = new ElseClauseSyntax(elseKeyword, elseBlock);
        }

        return new IfStatementSyntax(ifKeyword, openParen, condition, closeParen, thenBlock, elseClause);
    }

    private StatementSyntax ParseWhileStatement()
    {
        var whileKeyword = MatchToken(SyntaxKind.WhileKeyword);
        var openParen = MatchToken(SyntaxKind.OpenParenToken);
        var condition = ParseExpression();
        var closeParen = MatchToken(SyntaxKind.CloseParenToken);
        var body = ParseBlockStatement();

        return new WhileStatementSyntax(whileKeyword, openParen, condition, closeParen, body);
    }

    private ExpressionSyntax ParseExpression(int parentPrecedence = 0)
    {
        ExpressionSyntax left;
        var unaryPrecedence = SyntaxFacts.GetUnaryOperatorPrecedence(Current.Kind);
        if (unaryPrecedence != 0 && unaryPrecedence >= parentPrecedence)
        {
            var operatorToken = NextToken();
            var operand = ParseExpression(unaryPrecedence);
            left = new UnaryExpressionSyntax(operatorToken, operand);
        }
        else
        {
            left = ParsePrimaryExpression();
        }

        while (true)
        {
            var precedence = SyntaxFacts.GetBinaryOperatorPrecedence(Current.Kind);
            if (precedence == 0 || precedence <= parentPrecedence)
            {
                break;
            }

            var operatorToken = NextToken();
            var right = ParseExpression(precedence);
            left = new BinaryExpressionSyntax(left, operatorToken, right);
        }

        return left;
    }

    private ExpressionSyntax ParsePrimaryExpression()
    {
        return Current.Kind switch
        {
            SyntaxKind.OpenParenToken => ParseParenthesizedExpression(),
            SyntaxKind.TrueKeyword => ParseBooleanLiteral(true),
            SyntaxKind.FalseKeyword => ParseBooleanLiteral(false),
            SyntaxKind.IntLiteralToken => ParseLiteralExpression(),
            SyntaxKind.StringLiteralToken => ParseLiteralExpression(),
            SyntaxKind.IdentifierToken => ParseNameExpression(),
            _ => ParseMissingExpression(),
        };
    }

    private ExpressionSyntax ParseParenthesizedExpression()
    {
        var openParen = MatchToken(SyntaxKind.OpenParenToken);
        var expression = ParseExpression();
        var closeParen = MatchToken(SyntaxKind.CloseParenToken);
        return new ParenthesizedExpressionSyntax(openParen, expression, closeParen);
    }

    private ExpressionSyntax ParseBooleanLiteral(bool value)
    {
        var expectedKind = value ? SyntaxKind.TrueKeyword : SyntaxKind.FalseKeyword;
        var token = MatchToken(expectedKind);
        return new LiteralExpressionSyntax(token, value);
    }

    private ExpressionSyntax ParseLiteralExpression()
    {
        var token = NextToken();
        return new LiteralExpressionSyntax(token, token.Value);
    }

    private ExpressionSyntax ParseNameExpression()
    {
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        return new NameExpressionSyntax(identifier);
    }

    private ExpressionSyntax ParseMissingExpression()
    {
        var span = new TextSpan(Current.Position, Math.Max(1, Current.Text.Length));
        _diagnostics.Report(
            DiagnosticCodes.ExpectedExpression,
            $"Expected expression but found '{Current.Kind}'.",
            _source,
            span);

        var missingToken = new SyntaxToken(SyntaxKind.IdentifierToken, Current.Position, string.Empty, null);
        return new NameExpressionSyntax(missingToken);
    }
}
