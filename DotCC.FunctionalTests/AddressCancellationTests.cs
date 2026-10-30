using System;
using System.IO;
using System.Linq;
using DotCC;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class AddressCancellationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Dereference_of_address_preserves_storage_casts_and_single_evaluation(bool objects)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-address-cancellation-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "main.c");
            File.WriteAllText(path, """
                #define STORE(p, v) (*(p) = (v))
                struct State { int nearlyFull; volatile int pending; } state;
                int calls;
                struct State *next(void) { ++calls; return &state; }
                int *current;
                int main(void) {
                    STORE(&state.nearlyFull, 1);
                    if (*(&state.nearlyFull) != 1) return 1;
                    STORE(&state.nearlyFull, 0);
                    STORE(&next()->pending, 7);
                    int read = *(&next()->pending);
                    if (read != 7 || calls != 2) return 2;
                    *(unsigned int *)&state.nearlyFull = 0x80000001u;
                    if (state.nearlyFull >= 0 || *(unsigned int *)&state.nearlyFull != 0x80000001u) return 3;
                    int values[2] = {3, 5};
                    int index = 0;
                    STORE(&values[index++], 11);
                    if (index != 1 || values[0] != 11 || *(&values[1]) != 5) return 4;
                    int *pointer = values;
                    *(&pointer) = &values[1];
                    *(&current) = pointer;
                    if (*pointer != 5 || *current != 5) return 5;
                    _Atomic int atomic = 0;
                    *(&atomic) = 13;
                    if (*(&atomic) != 13) return 6;
                    return 0;
                }
                """);
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.o");
                File.WriteAllText(fragment, Compiler.EmitObject(path));
                path = fragment;
            }
            var generated = objects ? Compiler.LinkObjects([path]) : Compiler.EmitCSharp([path]);
            CSharpSyntaxTree.ParseText(generated).GetRoot().DescendantNodes()
                .OfType<PrefixUnaryExpressionSyntax>().Any(expression =>
                    expression.Kind() == SyntaxKind.PointerIndirectionExpression
                    && expression.Operand.Kind() == SyntaxKind.AddressOfExpression).ShouldBeFalse();
            generated.ShouldContain("__dotcc_globalAddress_state");
            generated.ShouldContain("*(uint*)");
            generated.ShouldContain("Volatile.Read(ref ");
            generated.ShouldContain("VolatileValue.Store(ref ");
            generated.ShouldContain("Atomic.Load(ref atomic)");
            generated.ShouldContain("Atomic.Store(ref atomic,");
            FixtureRunner.CompileAndRunCapturingExit(generated, Array.Empty<string>()).exit.ShouldBe(0);
        }
        finally { Directory.Delete(directory, true); }
    }
}
