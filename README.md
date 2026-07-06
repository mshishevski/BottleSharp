# BottleSharp

BottleSharp is a tiny C#-inspired educational programming language and compiler that lowers directly to CLR IL.

It is intentionally small and designed to demonstrate compiler engineering fundamentals with clean architecture, diagnostics, and tests.

## What BottleSharp Is

- A real compiler pipeline in C# (.NET 10): text -> lexer -> parser -> binder -> emitter.
- A tiny language with just enough features to show parsing, type checking, and IL emission.
- A command-line tool focused on `check` and `run` workflows.

## Example Syntax

```bsharp
print("Hello from BottleSharp");

let x = 10;
let y = 20;
let result = x + y;

print(result);
```

```bsharp
let temperature = 32;

if (temperature > 30) {
    print("hot");
} else {
    print("normal");
}

let i = 0;
while (i < 3) {
    print(i);
    i = i + 1;
}
```

## Compiler Pipeline

1. Source text and spans
2. Lexing
3. Parsing and AST
4. Binding and type checking
5. IL emission (Reflection.Emit)
6. Execution or persisted assembly output

## Getting Started

Requirements:

- .NET SDK 10.0+

Build and test:

```powershell
dotnet build BottleSharp.slnx
dotnet test BottleSharp.slnx --no-build
```

CLI usage during current phase:

```powershell
dotnet run --project src/BottleSharp.Cli -- check examples/hello.bsharp
dotnet run --project src/BottleSharp.Cli -- run examples/hello.bsharp
dotnet run --project src/BottleSharp.Cli -- run examples/booleans.bsharp
dotnet run --project src/BottleSharp.Cli -- run examples/strings.bsharp
dotnet run --project src/BottleSharp.Cli -- build examples/hello.bsharp -o artifacts/hello.dll
dotnet run --project src/BottleSharp.Cli -- help
```

## Repository Layout

- `src/BottleSharp.Compiler` - compiler implementation
- `src/BottleSharp.Cli` - CLI frontend
- `tests/BottleSharp.Compiler.Tests` - compiler unit tests
- `tests/BottleSharp.Cli.Tests` - CLI tests
- `examples` - BottleSharp sample programs
- `docs` - architecture, grammar, and roadmap notes

## Limitations (Current)

- `check` currently runs source-level, lexing, parsing, and semantic diagnostics.
- `run` compiles to CLR IL and executes via Reflection.Emit.
- `build -o` persists DLL and runtimeconfig artifacts, but the generated DLL does not yet include entry-point metadata for direct `dotnet <file.dll>` execution.
- persisted output can be loaded and invoked via reflection (`Program.Main`), or you can use `bottlesharp run` for direct execution.

## Roadmap

See:

- `docs/roadmap.md`
- `docs/architecture.md`
- `docs/grammar.md`

