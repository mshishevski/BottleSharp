using BottleSharp.Compiler.Text;

namespace BottleSharp.Compiler.Diagnostics;

public readonly struct TextLocation
{
    public TextLocation(SourceText source, TextSpan span)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
        Span = span;
    }

    public SourceText Source { get; }

    public TextSpan Span { get; }

    public int StartLine => Source.GetLineIndex(Span.Start);

    public int StartColumn => Span.Start - Source.Lines[StartLine].Start;
}
