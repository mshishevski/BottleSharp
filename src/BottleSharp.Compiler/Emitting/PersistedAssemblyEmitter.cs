using System.Reflection;
using System.Reflection.Emit;
using BottleSharp.Compiler.Binding;
using BottleSharp.Compiler.Syntax;

namespace BottleSharp.Compiler.Emitting;

public static class PersistedAssemblyEmitter
{
    private static readonly MethodInfo StringConcatMethod = typeof(string).GetMethod(nameof(string.Concat), [typeof(string), typeof(string)])
        ?? throw new InvalidOperationException("Could not resolve string.Concat(string, string).");

    private static readonly MethodInfo StringEqualsMethod = typeof(string).GetMethod("op_Equality", [typeof(string), typeof(string)])
        ?? throw new InvalidOperationException("Could not resolve string equality operator.");

    private static readonly MethodInfo StringNotEqualsMethod = typeof(string).GetMethod("op_Inequality", [typeof(string), typeof(string)])
        ?? throw new InvalidOperationException("Could not resolve string inequality operator.");

    private static readonly MethodInfo WriteLineIntMethod = typeof(Console).GetMethod(nameof(Console.WriteLine), [typeof(int)])
        ?? throw new InvalidOperationException("Could not resolve Console.WriteLine(int).");

    private static readonly MethodInfo WriteLineStringMethod = typeof(Console).GetMethod(nameof(Console.WriteLine), [typeof(string)])
        ?? throw new InvalidOperationException("Could not resolve Console.WriteLine(string).");

    private static readonly MethodInfo WriteLineBoolMethod = typeof(Console).GetMethod(nameof(Console.WriteLine), [typeof(bool)])
        ?? throw new InvalidOperationException("Could not resolve Console.WriteLine(bool).");

    public static void Save(BoundBlockStatement program, string outputPath)
    {
        if (program is null)
        {
            throw new ArgumentNullException(nameof(program));
        }

        if (string.IsNullOrWhiteSpace(outputPath))
        {
            throw new ArgumentException("Output path is required.", nameof(outputPath));
        }

        if (!string.Equals(Path.GetExtension(outputPath), ".dll", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Persisted build output must use the .dll extension.");
        }

        var fullOutputPath = Path.GetFullPath(outputPath);
        var directory = Path.GetDirectoryName(fullOutputPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var assemblyName = new AssemblyName(Path.GetFileNameWithoutExtension(fullOutputPath));
        var assemblyBuilder = new PersistedAssemblyBuilder(assemblyName, typeof(object).Assembly, []);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name ?? "BottleSharpProgram");
        var typeBuilder = moduleBuilder.DefineType("Program", TypeAttributes.Public | TypeAttributes.Class);

        var mainMethod = typeBuilder.DefineMethod(
            "Main",
            MethodAttributes.Public | MethodAttributes.Static,
            typeof(void),
            Type.EmptyTypes);

        var il = mainMethod.GetILGenerator();
        var emitter = new EmitterImpl(il);
        emitter.EmitStatement(program);
        il.Emit(OpCodes.Ret);

        typeBuilder.CreateType();
        assemblyBuilder.Save(fullOutputPath);

        WriteRuntimeConfig(fullOutputPath);
    }

    private static void WriteRuntimeConfig(string outputPath)
    {
        var runtimeConfigPath = Path.ChangeExtension(outputPath, ".runtimeconfig.json");
        var runtimeConfig =
            "{\n" +
            "  \"runtimeOptions\": {\n" +
            "    \"tfm\": \"net10.0\",\n" +
            "    \"framework\": {\n" +
            "      \"name\": \"Microsoft.NETCore.App\",\n" +
            "      \"version\": \"10.0.0\"\n" +
            "    }\n" +
            "  }\n" +
            "}\n";

        File.WriteAllText(runtimeConfigPath, runtimeConfig);
    }

    private sealed class EmitterImpl
    {
        private readonly ILGenerator _il;
        private readonly Dictionary<VariableSymbol, LocalBuilder> _locals = [];

        public EmitterImpl(ILGenerator il)
        {
            _il = il;
        }

        public void EmitStatement(BoundStatement statement)
        {
            switch (statement)
            {
                case BoundBlockStatement block:
                    foreach (var child in block.Statements)
                    {
                        EmitStatement(child);
                    }

                    break;
                case BoundVariableDeclarationStatement variableDeclaration:
                    EmitVariableDeclaration(variableDeclaration);
                    break;
                case BoundAssignmentStatement assignment:
                    EmitAssignment(assignment);
                    break;
                case BoundPrintStatement print:
                    EmitPrint(print);
                    break;
                case BoundIfStatement ifStatement:
                    EmitIf(ifStatement);
                    break;
                case BoundWhileStatement whileStatement:
                    EmitWhile(whileStatement);
                    break;
                default:
                    throw new InvalidOperationException($"Unexpected bound statement '{statement.Kind}'.");
            }
        }

        private void EmitVariableDeclaration(BoundVariableDeclarationStatement statement)
        {
            EmitExpression(statement.Initializer);

            var localType = GetClrType(statement.Variable.Type);
            var local = _il.DeclareLocal(localType);
            _locals.Add(statement.Variable, local);
            _il.Emit(OpCodes.Stloc, local);
        }

        private void EmitAssignment(BoundAssignmentStatement statement)
        {
            EmitExpression(statement.Expression);
            _il.Emit(OpCodes.Stloc, LookupLocal(statement.Variable));
        }

        private void EmitPrint(BoundPrintStatement statement)
        {
            EmitExpression(statement.Expression);

            var method = statement.Expression.Type switch
            {
                var t when t == TypeSymbol.Int => WriteLineIntMethod,
                var t when t == TypeSymbol.String => WriteLineStringMethod,
                var t when t == TypeSymbol.Bool => WriteLineBoolMethod,
                _ => throw new InvalidOperationException($"Unsupported print type '{statement.Expression.Type}'."),
            };

            _il.Emit(OpCodes.Call, method);
        }

        private void EmitIf(BoundIfStatement statement)
        {
            var elseLabel = _il.DefineLabel();
            var endLabel = _il.DefineLabel();

            EmitExpression(statement.Condition);
            _il.Emit(OpCodes.Brfalse, elseLabel);

            EmitStatement(statement.ThenStatement);
            _il.Emit(OpCodes.Br, endLabel);

            _il.MarkLabel(elseLabel);
            if (statement.ElseStatement is not null)
            {
                EmitStatement(statement.ElseStatement);
            }

            _il.MarkLabel(endLabel);
        }

        private void EmitWhile(BoundWhileStatement statement)
        {
            var startLabel = _il.DefineLabel();
            var endLabel = _il.DefineLabel();

            _il.MarkLabel(startLabel);
            EmitExpression(statement.Condition);
            _il.Emit(OpCodes.Brfalse, endLabel);

            EmitStatement(statement.Body);
            _il.Emit(OpCodes.Br, startLabel);

            _il.MarkLabel(endLabel);
        }

        private void EmitExpression(BoundExpression expression)
        {
            switch (expression)
            {
                case BoundLiteralExpression literal:
                    EmitLiteral(literal);
                    break;
                case BoundVariableExpression variable:
                    _il.Emit(OpCodes.Ldloc, LookupLocal(variable.Variable));
                    break;
                case BoundUnaryExpression unary:
                    EmitUnary(unary);
                    break;
                case BoundBinaryExpression binary:
                    EmitBinary(binary);
                    break;
                case BoundErrorExpression:
                    throw new InvalidOperationException("Cannot emit error expression.");
                default:
                    throw new InvalidOperationException($"Unexpected bound expression '{expression.Kind}'.");
            }
        }

        private void EmitLiteral(BoundLiteralExpression literal)
        {
            if (literal.Type == TypeSymbol.Int)
            {
                _il.Emit(OpCodes.Ldc_I4, (int)(literal.Value ?? 0));
                return;
            }

            if (literal.Type == TypeSymbol.String)
            {
                if (literal.Value is string stringValue)
                {
                    _il.Emit(OpCodes.Ldstr, stringValue);
                }
                else
                {
                    _il.Emit(OpCodes.Ldnull);
                }

                return;
            }

            if (literal.Type == TypeSymbol.Bool)
            {
                _il.Emit((bool)(literal.Value ?? false) ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                return;
            }

            throw new InvalidOperationException($"Unsupported literal type '{literal.Type}'.");
        }

        private void EmitUnary(BoundUnaryExpression unary)
        {
            EmitExpression(unary.Operand);

            switch (unary.Op.SyntaxKind)
            {
                case SyntaxKind.BangToken:
                    _il.Emit(OpCodes.Ldc_I4_0);
                    _il.Emit(OpCodes.Ceq);
                    break;
                case SyntaxKind.MinusToken:
                    _il.Emit(OpCodes.Neg);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported unary operator '{unary.Op.SyntaxKind}'.");
            }
        }

        private void EmitBinary(BoundBinaryExpression binary)
        {
            if (binary.Op.SyntaxKind == SyntaxKind.AmpersandAmpersandToken)
            {
                EmitLogicalAnd(binary);
                return;
            }

            if (binary.Op.SyntaxKind == SyntaxKind.PipePipeToken)
            {
                EmitLogicalOr(binary);
                return;
            }

            EmitExpression(binary.Left);
            EmitExpression(binary.Right);

            if (binary.Left.Type == TypeSymbol.String)
            {
                EmitStringBinary(binary.Op.SyntaxKind);
                return;
            }

            if (binary.Left.Type == TypeSymbol.Int)
            {
                EmitIntBinary(binary.Op.SyntaxKind);
                return;
            }

            if (binary.Left.Type == TypeSymbol.Bool)
            {
                EmitBoolBinary(binary.Op.SyntaxKind);
                return;
            }

            throw new InvalidOperationException($"Unsupported binary operand type '{binary.Left.Type}'.");
        }

        private void EmitStringBinary(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.PlusToken:
                    _il.Emit(OpCodes.Call, StringConcatMethod);
                    break;
                case SyntaxKind.EqualsEqualsToken:
                    _il.Emit(OpCodes.Call, StringEqualsMethod);
                    break;
                case SyntaxKind.BangEqualsToken:
                    _il.Emit(OpCodes.Call, StringNotEqualsMethod);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported string operator '{kind}'.");
            }
        }

        private void EmitIntBinary(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.PlusToken:
                    _il.Emit(OpCodes.Add);
                    break;
                case SyntaxKind.MinusToken:
                    _il.Emit(OpCodes.Sub);
                    break;
                case SyntaxKind.StarToken:
                    _il.Emit(OpCodes.Mul);
                    break;
                case SyntaxKind.SlashToken:
                    _il.Emit(OpCodes.Div);
                    break;
                case SyntaxKind.EqualsEqualsToken:
                    _il.Emit(OpCodes.Ceq);
                    break;
                case SyntaxKind.BangEqualsToken:
                    EmitNotEqual();
                    break;
                case SyntaxKind.LessToken:
                    _il.Emit(OpCodes.Clt);
                    break;
                case SyntaxKind.LessOrEqualsToken:
                    _il.Emit(OpCodes.Cgt);
                    _il.Emit(OpCodes.Ldc_I4_0);
                    _il.Emit(OpCodes.Ceq);
                    break;
                case SyntaxKind.GreaterToken:
                    _il.Emit(OpCodes.Cgt);
                    break;
                case SyntaxKind.GreaterOrEqualsToken:
                    _il.Emit(OpCodes.Clt);
                    _il.Emit(OpCodes.Ldc_I4_0);
                    _il.Emit(OpCodes.Ceq);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported int operator '{kind}'.");
            }
        }

        private void EmitBoolBinary(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.EqualsEqualsToken:
                    _il.Emit(OpCodes.Ceq);
                    break;
                case SyntaxKind.BangEqualsToken:
                    EmitNotEqual();
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported bool operator '{kind}'.");
            }
        }

        private void EmitNotEqual()
        {
            _il.Emit(OpCodes.Ceq);
            _il.Emit(OpCodes.Ldc_I4_0);
            _il.Emit(OpCodes.Ceq);
        }

        private void EmitLogicalAnd(BoundBinaryExpression binary)
        {
            var falseLabel = _il.DefineLabel();
            var endLabel = _il.DefineLabel();

            EmitExpression(binary.Left);
            _il.Emit(OpCodes.Brfalse, falseLabel);
            EmitExpression(binary.Right);
            _il.Emit(OpCodes.Br, endLabel);

            _il.MarkLabel(falseLabel);
            _il.Emit(OpCodes.Ldc_I4_0);

            _il.MarkLabel(endLabel);
        }

        private void EmitLogicalOr(BoundBinaryExpression binary)
        {
            var trueLabel = _il.DefineLabel();
            var endLabel = _il.DefineLabel();

            EmitExpression(binary.Left);
            _il.Emit(OpCodes.Brtrue, trueLabel);
            EmitExpression(binary.Right);
            _il.Emit(OpCodes.Br, endLabel);

            _il.MarkLabel(trueLabel);
            _il.Emit(OpCodes.Ldc_I4_1);

            _il.MarkLabel(endLabel);
        }

        private LocalBuilder LookupLocal(VariableSymbol variable)
        {
            if (_locals.TryGetValue(variable, out var local))
            {
                return local;
            }

            throw new InvalidOperationException($"Variable '{variable.Name}' is not mapped to a local slot.");
        }

        private static Type GetClrType(TypeSymbol type)
        {
            if (type == TypeSymbol.Int)
            {
                return typeof(int);
            }

            if (type == TypeSymbol.String)
            {
                return typeof(string);
            }

            if (type == TypeSymbol.Bool)
            {
                return typeof(bool);
            }

            throw new InvalidOperationException($"Unsupported CLR type mapping for '{type}'.");
        }
    }
}
