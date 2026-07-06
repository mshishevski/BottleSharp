namespace BottleSharp.Compiler.Text;

public readonly struct TextLine
{
    public TextLine(int start, int length, int lengthIncludingLineBreak)
    {
        Start = start;
        Length = length;
        LengthIncludingLineBreak = lengthIncludingLineBreak;
    }

    public int Start { get; }

    public int Length { get; }

    public int LengthIncludingLineBreak { get; }

    public int End => Start + Length;
}
