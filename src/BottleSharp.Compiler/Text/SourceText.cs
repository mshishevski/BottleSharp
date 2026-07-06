using System.Collections.Immutable;

namespace BottleSharp.Compiler.Text;

public sealed class SourceText
{
    private readonly string _text;

    public SourceText(string text, string? filePath = null)
    {
        _text = text ?? throw new ArgumentNullException(nameof(text));
        FilePath = filePath ?? string.Empty;
        Lines = ParseLines(_text);
    }

    public string FilePath { get; }

    public int Length => _text.Length;

    public ImmutableArray<TextLine> Lines { get; }

    public char this[int index] => _text[index];

    public override string ToString() => _text;

    public string ToString(TextSpan span)
    {
        return _text.Substring(span.Start, span.Length);
    }

    public int GetLineIndex(int position)
    {
        if (position < 0 || position > _text.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }

        var lower = 0;
        var upper = Lines.Length - 1;

        while (lower <= upper)
        {
            var index = lower + ((upper - lower) / 2);
            var line = Lines[index];

            if (position < line.Start)
            {
                upper = index - 1;
            }
            else if (position >= line.Start + line.LengthIncludingLineBreak)
            {
                lower = index + 1;
            }
            else
            {
                return index;
            }
        }

        return Lines.Length - 1;
    }

    private static ImmutableArray<TextLine> ParseLines(string text)
    {
        var lines = ImmutableArray.CreateBuilder<TextLine>();
        var lineStart = 0;
        var position = 0;

        while (position < text.Length)
        {
            var lineBreakWidth = GetLineBreakWidth(text, position);

            if (lineBreakWidth == 0)
            {
                position++;
                continue;
            }

            var lineLength = position - lineStart;
            lines.Add(new TextLine(lineStart, lineLength, lineLength + lineBreakWidth));

            position += lineBreakWidth;
            lineStart = position;
        }

        if (position >= lineStart)
        {
            var lineLength = position - lineStart;
            lines.Add(new TextLine(lineStart, lineLength, lineLength));
        }

        return lines.ToImmutable();
    }

    private static int GetLineBreakWidth(string text, int position)
    {
        var current = text[position];
        var lookahead = position + 1 >= text.Length ? '\0' : text[position + 1];

        if (current == '\r' && lookahead == '\n')
        {
            return 2;
        }

        if (current == '\r' || current == '\n')
        {
            return 1;
        }

        return 0;
    }
}
