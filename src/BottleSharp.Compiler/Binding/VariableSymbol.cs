namespace BottleSharp.Compiler.Binding;

public sealed class VariableSymbol
{
	public VariableSymbol(string name, TypeSymbol type)
	{
		Name = name;
		Type = type;
	}

	public string Name { get; }

	public TypeSymbol Type { get; }
}
