using System;
using System.IO;
using System.Linq;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class PrivateTaggedVariantsTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Private_recursive_variants_cross_an_opaque_capsule_without_losing_callbacks(bool objects, bool reverse)
    {
        WithDirectory(directory =>
        {
            File.WriteAllText(Path.Combine(directory, "codec.h"), """
                struct State;
                typedef struct Codec {
                    int (*read)(struct State *);
                    struct State *state;
                    struct Codec *self;
                    struct State *slots[2];
                } Codec;
                """);
            var paths = Sources(directory,
                """
                #include "codec.h"
                void *first(void); void *second(void);
                int first_size(void); int second_size(void);
                int main(void) {
                    Codec *a=first(), *b=second();
                    return first_size()==16 && second_size()==24 && a!=b && a==a->self && b==b->self
                        && a->state==a->slots[0] && b->state==b->slots[1] && a->read!=b->read
                        && a->read(a->state)==7 && b->read(b->state)==42 ? 0:1;
                }
                """,
                """
                #include "codec.h"
                typedef struct State { int value; Codec *codec; } State;
                static int read_first(State *p) { return p->value + (p->codec->state==p ? 0:100); }
                static State first_state = {7, 0};
                static Codec first_codec = {read_first, &first_state, &first_codec, {&first_state, &first_state}};
                void *first(void) { first_state.codec=&first_codec; return &first_codec; }
                int first_size(void) { return sizeof(State); }
                """,
                """
                #include "codec.h"
                typedef struct State { int value; Codec *codec; long extension; } State;
                static int read_second(State *p) { return p->value + p->extension + (p->codec->state==p ? 0:100); }
                static State second_state = {19, 0, 23};
                static Codec second_codec = {read_second, &second_state, &second_codec, {&second_state, &second_state}};
                void *second(void) { second_state.codec=&second_codec; return &second_codec; }
                int second_size(void) { return sizeof(State); }
                """);
            if (objects) paths = Objects(paths);
            if (reverse) Array.Reverse(paths);
            var text = objects ? Compiler.LinkObjects(paths) : Compiler.EmitCSharp(paths);
            FixtureRunner.CompileAndRunCapturingExit(text, Array.Empty<string>()).exit.ShouldBe(0);
        });
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Externally_shared_pointer_and_callback_graphs_reject_incompatible_complete_layouts(bool objects, bool reverse)
    {
        WithDirectory(directory =>
        {
            var paths = Sources(directory,
                "struct State { int value; }; struct Codec { int (*read)(struct State *); }; extern struct Codec *shared; int main(void) { return 0; }",
                "struct State { long value; }; struct Codec { int (*read)(struct State *); }; struct Codec *shared;");
            if (objects) paths = Objects(paths);
            if (reverse) Array.Reverse(paths);
            Should.Throw<CompileException>(() => objects ? Compiler.LinkObjects(paths) : Compiler.EmitCSharp(paths))
                .Message.ShouldContain("conflicting aggregate declarations");
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Variant_tag_spelling_does_not_rewrite_a_member_or_literal(bool reverse)
    {
        WithDirectory(directory =>
        {
            var paths = Sources(directory,
                "struct State { int State; char *text; }; static struct State first_state={7,\"State\"}; int other(void); int main(void) { return first_state.State==7 && first_state.text[0]=='S' && other()==42 ? 0:1; }",
                "struct State { long State; }; static struct State second_state={42}; int other(void) { return second_state.State; }");
            paths = Objects(paths);
            if (reverse) Array.Reverse(paths);
            FixtureRunner.CompileAndRunCapturingExit(Compiler.LinkObjects(paths), Array.Empty<string>()).exit.ShouldBe(0);
        });
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Private_flexible_headers_keep_their_own_offset_metadata_and_pointer_projections(bool objects, bool reverse)
    {
        WithDirectory(directory =>
        {
            var paths = Sources(directory,
                "int first(void); int second(void); int main(void) { return first() && second() ? 0:1; }",
                """
                #include <stddef.h>
                struct Variant { int value; unsigned char tail[]; };
                static struct Variant first_state = {7};
                int first(void) { return sizeof(struct Variant)==4 && offsetof(struct Variant,tail)==4
                    && first_state.value==7 && first_state.tail==(unsigned char *)&first_state+4; }
                """,
                """
                #include <stddef.h>
                struct Variant { long prefix; int value; unsigned char tail[]; };
                static struct Variant second_state = {5,19};
                int second(void) { return sizeof(struct Variant)==16 && offsetof(struct Variant,tail)==12
                    && second_state.prefix==5 && second_state.value==19 && second_state.tail==(unsigned char *)&second_state+12; }
                """);
            if (objects) paths = Objects(paths);
            if (reverse) Array.Reverse(paths);
            var text = objects ? Compiler.LinkObjects(paths) : Compiler.EmitCSharp(paths);
            FixtureRunner.CompileAndRunCapturingExit(text, Array.Empty<string>()).exit.ShouldBe(0);
        });
    }

    private static string[] Sources(string directory, params string[] sources) => sources.Select((text, i) =>
    {
        var path = Path.Combine(directory, "unit" + i + ".c"); File.WriteAllText(path, text); return path;
    }).ToArray();
    private static string[] Objects(string[] paths) => paths.Select(path =>
    {
        var obj = path + ".cs"; File.WriteAllText(obj, Compiler.EmitObject(path)); return obj;
    }).ToArray();
    private static void WithDirectory(Action<string> action)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-private-variants-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try { action(directory); } finally { Directory.Delete(directory, true); }
    }
}
