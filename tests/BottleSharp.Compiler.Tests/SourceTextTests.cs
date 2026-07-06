using BottleSharp.Compiler.Text;

namespace BottleSharp.Compiler.Tests;

public sealed class SourceTextTests
{
    [Fact]
    public void GetLineIndex_MapsPositionToExpectedLine()
    {
        var source = new SourceText("a\nb\r\nc");

        Assert.Equal(0, source.GetLineIndex(0));
        Assert.Equal(0, source.GetLineIndex(1));
        Assert.Equal(1, source.GetLineIndex(2));
        Assert.Equal(1, source.GetLineIndex(4));
        Assert.Equal(2, source.GetLineIndex(5));
    }
}
