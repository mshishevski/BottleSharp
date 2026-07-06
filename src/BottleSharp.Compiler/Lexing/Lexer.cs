using System.Collections.Immutable;
using BottleSharp.Compiler.Diagnostics;
using BottleSharp.Compiler.Syntax;
using BottleSharp.Compiler.Text;

namespace BottleSharp.Compiler.Lexing;

public sealed class Lexer
{
    private readonly SourceText _source;
    private readonly DiagnosticBag _diagnostics = new();
    private readonly List<SyntaxToken> _tokens = [];

    private int _position;

    public Lexer(SourceText source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
    }

    public LexResult Lex()
    {
        while (Current != '\0')
        {
            if (char.IsWhiteSpace(Current))
            {
                Next();
                continue;
            }

            var token = LexToken();
            if (token.Kind != SyntaxKind.BadToken)
            {
                _tokens.Add(token);
            }
        }

        _tokens.Add(new SyntaxToken(SyntaxKind.EndOfFileToken, _position, string.Empty, null));
        return new LexResult(_tokens.ToImmutableArray(), _diagnostics.ToImmutableArray());
    }

    private char Current => Peek(0);

    private char Lookahead => Peek(1);

    private char Peek(int offset)
    {
        var index = _position + offset;
        if (index >= _source.Length)
        {
            return '\0';
        }

        return _source[index];
    }

    private void Next()
    {
        _position++;
    }

    private SyntaxToken LexToken()
    {
        var start = _position;

        if (char.IsDigit(Current))
        {
            return LexNumberToken(start);
        }

        if (Current == '"')
        {
            return LexStringToken(start);
        }

        if (char.IsLetter(Current) || Current == '_')
        {
            return LexIdentifierOrKeywordToken(start);
        }

        switch (Current)
        {
            case '+':
                Next();
                return new SyntaxToken(SyntaxKind.PlusToken, start, "+", null);
            case '-':
                Next();
                return new SyntaxToken(SyntaxKind.MinusToken, start, "-", null);
            case '*':
                Next();
                return new SyntaxToken(SyntaxKind.StarToken, start, "*", null);
            case '/':
                Next();
                return new SyntaxToken(SyntaxKind.SlashToken, start, "/", null);
            case '!':
                if (Lookahead == '=')
                {
                    Next();
                    Next();
                    return new SyntaxToken(SyntaxKind.BangEqualsToken, start, "!=", null);
                }

                Next();
                return new SyntaxToken(SyntaxKind.BangToken, start, "!", null);
            case '=':
                if (Lookahead == '=')
                {
                    Next();
                    Next();
                    return new SyntaxToken(SyntaxKind.EqualsEqualsToken, start, "==", null);
                }

                Next();
                return new SyntaxToken(SyntaxKind.EqualsToken, start, "=", null);
            case '<':
                if (Lookahead == '=')
                {
                    Next();
                    Next();
                    return new SyntaxToken(SyntaxKind.LessOrEqualsToken, start, "<=", null);
                }

                Next();
                return new SyntaxToken(SyntaxKind.LessToken, start, "<", null);
            case '>':
                if (Lookahead == '=')
                {
                    Next();
                    Next();
                    return new SyntaxToken(SyntaxKind.GreaterOrEqualsToken, start, ">=", null);
                }

                Next();
                return new SyntaxToken(SyntaxKind.GreaterToken, start, ">", null);
            case '&':
                if (Lookahead == '&')
                {
                    Next();
                    Next();
                    return new SyntaxToken(SyntaxKind.AmpersandAmpersandToken, start, "&&", null);
                }

                return LexInvalidSingleCharToken(start, "&", "Unexpected character '&'. Did you mean '&&'?");
            case '|':
                if (Lookahead == '|')
                {
                    Next();
                    Next();
                    return new SyntaxToken(SyntaxKind.PipePipeToken, start, "||", null);
                }

                return LexInvalidSingleCharToken(start, "|", "Unexpected character '|'. Did you mean '||'?");
            case '(':
                Next();
                return new SyntaxToken(SyntaxKind.OpenParenToken, start, "(", null);
            case ')':
                Next();
                return new SyntaxToken(SyntaxKind.CloseParenToken, start, ")", null);
            case '{':
                Next();
                return new SyntaxToken(SyntaxKind.OpenBraceToken, start, "{", null);
            case '}':
                Next();
                return new SyntaxToken(SyntaxKind.CloseBraceToken, start, "}", null);
            case ';':
                Next();
                return new SyntaxToken(SyntaxKind.SemicolonToken, start, ";", null);
            case ',':
                Next();
                return new SyntaxToken(SyntaxKind.CommaToken, start, ",", null);
            default:
                return LexInvalidSingleCharToken(start, Current.ToString(), $"Unexpected character '{Current}'.");
        }
    }

    private SyntaxToken LexIdentifierOrKeywordToken(int start)
    {
        while (char.IsLetterOrDigit(Current) || Current == '_')
        {
            Next();
        }

        var text = _source.ToString(TextSpan.FromBounds(start, _position));
        var kind = SyntaxFacts.GetKeywordKind(text);

        object? value = null;
        if (kind == SyntaxKind.TrueKeyword)
        {
            value = true;
        }
        else if (kind == SyntaxKind.FalseKeyword)
        {
            value = false;
        }

        return new SyntaxToken(kind, start, text, value);
    }

    private SyntaxToken LexNumberToken(int start)
    {
        while (char.IsDigit(Current))
        {
            Next();
        }

        var text = _source.ToString(TextSpan.FromBounds(start, _position));
        if (!int.TryParse(text, out var value))
        {
            _diagnostics.Report(
                DiagnosticCodes.InvalidIntegerLiteral,
                $"Invalid integer literal '{text}'.",
                _source,
                TextSpan.FromBounds(start, _position));

            return new SyntaxToken(SyntaxKind.IntLiteralToken, start, text, 0);
        }

        return new SyntaxToken(SyntaxKind.IntLiteralToken, start, text, value);
    }

    private SyntaxToken LexStringToken(int start)
    {
        Next();

        while (Current != '\0' && Current != '"' && Current != '\r' && Current != '\n')
        {
            Next();
        }

        if (Current != '"')
        {
            _diagnostics.Report(
                DiagnosticCodes.UnterminatedString,
                "Unterminated string literal.",
                _source,
                TextSpan.FromBounds(start, _position));

            var rawUnterminatedText = _source.ToString(TextSpan.FromBounds(start, _position));
            return new SyntaxToken(SyntaxKind.StringLiteralToken, start, rawUnterminatedText, rawUnterminatedText.Trim('"'));
        }

        Next();

        var fullText = _source.ToString(TextSpan.FromBounds(start, _position));
        var value = fullText.Substring(1, fullText.Length - 2);

        return new SyntaxToken(SyntaxKind.StringLiteralToken, start, fullText, value);
    }

    private SyntaxToken LexInvalidSingleCharToken(int start, string text, string message)
    {
        _diagnostics.Report(
            DiagnosticCodes.UnexpectedCharacter,
            message,
            _source,
            new TextSpan(start, text.Length));

        Next();
        return new SyntaxToken(SyntaxKind.BadToken, start, text, null);
    }
}
