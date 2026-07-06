namespace BottleSharp.Compiler.Binding;

public sealed class BoundScope
{
    private readonly Dictionary<string, VariableSymbol> _variables = [];

    public BoundScope(BoundScope? parent)
    {
        Parent = parent;
    }

    public BoundScope? Parent { get; }

    public bool TryDeclare(VariableSymbol variable)
    {
        return _variables.TryAdd(variable.Name, variable);
    }

    public bool TryLookup(string name, out VariableSymbol? variable)
    {
        if (_variables.TryGetValue(name, out var local))
        {
            variable = local;
            return true;
        }

        if (Parent is not null)
        {
            return Parent.TryLookup(name, out variable);
        }

        variable = null;
        return false;
    }
}
