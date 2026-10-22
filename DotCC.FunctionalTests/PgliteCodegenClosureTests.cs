using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class PgliteCodegenClosureTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Gnu_unreachable_terminates_switch_and_void_conditional_paths(bool objects)
    {
        Run(objects, """
            int pick(int value) {
                switch (value) { case 1: return 42; default: __builtin_unreachable(); }
            }
            void check(int value) { value ? (void)0 : __builtin_unreachable(); }
            int unreachable_branches(int value) {
                value ? __builtin_unreachable() : __builtin_unreachable();
            }
            int unreachable_comma(void) { ((void)0, __builtin_unreachable()); }
            int main(void) { check(1); return pick(1) == 42 ? 0 : 1; }
            """);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Bound_callback_named_like_gnu_unreachable_keeps_normal_semantics(bool objects)
    {
        Run(objects, """
            static int calls;
            int increment(int n) { ++calls; return n + 1; }
            int main(void) {
                int (*__builtin_unreachable)(int) = increment;
                return __builtin_unreachable(41) == 42 && calls == 1 ? 0 : 1;
            }
            """);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Pointer_storage_assignment_and_comma_for_clauses_discard_only_values(bool objects)
    {
        Run(objects, """
            struct Link { int n; struct Link *next; };
            static struct Link tail = {23,0}, head = {19,&tail};
            static struct Link *cursor;
            int main(void) {
                int count=0, total=0;
                for (cursor=&head; cursor; cursor=cursor->next) total+=cursor->n;
                for (count=0, cursor=&head; cursor; ++count, cursor=cursor->next) {}
                return total==42 && count==2 && cursor==0 ? 0 : 1;
            }
            """);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Addressed_static_compound_literals_keep_owned_writable_storage(bool objects)
    {
        Run(objects, """
            struct Item { int value; };
            static struct Item *items[] = { &(struct Item){19}, &(struct Item){23} };
            static void clobber(void) { volatile int scratch[100]; for(int i=0;i<100;i++) scratch[i]=i; }
            int main(void) {
                struct Item *first=items[0], *second=items[1];
                clobber();
                if(first==second || first->value+second->value!=42) return 1;
                first->value=41;
                clobber();
                return items[0]==first && items[1]==second && items[0]->value==41 ? 0 : 2;
            }
            """);
    }

    private static void Run(bool objects, string source)
    {
        string directory = Path.Combine(Path.GetTempPath(), "dotcc-pglite-codegen-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string path = Path.Combine(directory, "main.c");
            File.WriteAllText(path, source);
            string code;
            if (objects)
            {
                string obj = path + ".cs";
                File.WriteAllText(obj, Compiler.EmitObject(path));
                code = Compiler.LinkObjects(new[] { obj });
            }
            else code = Compiler.EmitCSharp(new[] { path });
            FixtureRunner.CompileAndRunCapturingExit(code, Array.Empty<string>()).exit.ShouldBe(0);
        }
        finally { Directory.Delete(directory, true); }
    }
}
