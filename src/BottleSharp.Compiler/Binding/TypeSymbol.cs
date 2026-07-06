namespace BottleSharp.Compiler.Binding;

public sealed class TypeSymbol
{
    private TypeSymbol(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public static TypeSymbol Int { get; } = new("int");

    public static TypeSymbol String { get; } = new("string");

    public static TypeSymbol Bool { get; } = new("bool");

    public static TypeSymbol Error { get; } = new("error");

    public override string ToString() => Name;
}
