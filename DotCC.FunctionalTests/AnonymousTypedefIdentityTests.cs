using System;
using System.IO;
using System.Linq;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class AnonymousTypedefIdentityTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Unrelated_aliases_keep_their_own_struct_and_union_layouts(bool objects, bool reverse)
    {
        Run(objects, reverse, "", new[] {
            """
            typedef struct { int value; } State;
            typedef union { int value; unsigned bits; } Value;
            static State state = { 7 };
            static Value value = { 9 };
            int second(void);
            int main(void) { State *p = &state; p->value += value.value; return sizeof(State)==4 && sizeof(Value)==4 && p==&state && p->value==16 && second()==42 ? 0 : 1; }
            """,
            """
            typedef union { long value; double number; } Value;
            typedef struct { long prefix; Value value; } State;
            static State second_state = { 11, { 31 } };
            int second(void) { State *p=&second_state; p->value.value++; return sizeof(State)==16 && sizeof(Value)==8 && p==&second_state ? (int)(p->prefix+p->value.value-1) : -1; }
            """
        });
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Shared_header_callbacks_forward_completion_and_addresses_agree(bool objects, bool reverse)
    {
        Run(objects, reverse, """
            struct Node;
            typedef union { long number; struct Node *node; } Value;
            typedef struct { Value value; int (*callback)(struct Node *); } Packet;
            Packet *get(void);
            int inspect(Packet *);
            int callback(struct Node *);
            """, new[] {
            """
            typedef struct { int unrelated; } Private;
            #include "shared.h"
            int main(void) { Packet *p=get(); return p==get() && p->callback==callback && inspect(p)==42 && p->callback(p->value.node)==42 ? 0 : 1; }
            """,
            """
            #include "shared.h"
            typedef struct { long unrelated[3]; } Private;
            struct Node { int value; struct Node *next; };
            static struct Node node = {42, &node};
            static Packet packet = {{0}, callback};
            int callback(struct Node *p) { return p==p->next ? p->value : -1; }
            Packet *get(void) { packet.value.node=&node; return &packet; }
            int inspect(Packet *p) { return p->callback(p->value.node); }
            """
        });
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Shared_anonymous_definition_with_genuinely_conflicting_configuration_is_rejected(bool reverse, bool union)
    {
        WithDirectory(directory => {
            File.WriteAllText(Path.Combine(directory, "shared.h"), "typedef AGG { FIELD value; } Shared;\n");
            var sources = new[] { "#define AGG struct\n#define FIELD int\n#include \"shared.h\"\nint main(void) { return 0; }", (union ? "#define AGG union\n#define FIELD int\n" : "#define AGG struct\n#define FIELD long\n")+"#include \"shared.h\"\n" };
            var paths = Sources(directory, sources).Select(path => {
                var obj=path+".cs"; File.WriteAllText(obj,Compiler.EmitObject(path)); return obj;
            }).ToArray();
            if(reverse) Array.Reverse(paths);
            var error=Should.Throw<CompileException>(()=>Compiler.LinkObjects(paths));
            error.Message.ShouldContain("conflicting aggregate declarations");
            error.Message.ShouldContain(paths[0]); error.Message.ShouldContain(paths[1]);
        });
    }

    private static void Run(bool objects, bool reverse, string header, string[] sources) => WithDirectory(directory => {
        File.WriteAllText(Path.Combine(directory,"shared.h"),header);
        var paths=Sources(directory,sources);
        if(objects) paths=paths.Select(path=>{var obj=path+".cs";File.WriteAllText(obj,Compiler.EmitObject(path));return obj;}).ToArray();
        if(reverse) Array.Reverse(paths);
        var text=objects?Compiler.LinkObjects(paths):Compiler.EmitCSharp(paths);
        FixtureRunner.CompileAndRunCapturingExit(text,Array.Empty<string>()).exit.ShouldBe(0);
    });

    private static string[] Sources(string directory,string[] sources)=>sources.Select((source,index)=>{
        var path=Path.Combine(directory,"unit"+index+".c");File.WriteAllText(path,source);return path;
    }).ToArray();

    private static void WithDirectory(Action<string> action)
    {
        var directory=Path.Combine(Path.GetTempPath(),"dotcc-anonymous-typedef-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try { action(directory); } finally { Directory.Delete(directory,true); }
    }
}
