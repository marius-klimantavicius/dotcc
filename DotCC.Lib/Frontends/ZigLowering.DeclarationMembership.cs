using System;
using System.Collections.Generic;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    private readonly HashSet<string> _publicTopLevelDeclarations = new(StringComparer.Ordinal);

    // Source namespace membership observes headers, including generic functions
    // and unresolved aliases. It must not demand their signatures or bodies.
    private void IndexTopLevelMembership(IReadOnlyList<Item> declarations)
    {
        foreach (var declaration in declarations)
        {
            var visible = declaration.Content switch
            {
                Zig.PubFn or Zig.PubVar or Zig.PubContainer => true,
                Zig.InlineFn d => d.Arg0 is not null,
                Zig.NoinlineFn d => d.Arg0 is not null,
                Zig.ExportFn d => d.Arg0 is not null,
                Zig.ExportVar d => d.Arg0 is not null,
                Zig.ExternFnProto d => d.Arg0 is not null,
                Zig.ExternVar d => d.Arg0 is not null,
                Zig.ExternConst d => d.Arg0 is not null,
                _ => false,
            };
            if (!visible) continue;
            Item? token = Unwrap(declaration).Content switch
            {
                Zig.FnDef d => d.Arg1,
                Zig.ExternFnProto d => d.Arg4,
                Zig.ExternVar d => d.Arg5,
                Zig.ExternConst d => d.Arg4,
                Zig.ConstDecl d => d.Arg1,
                Zig.ConstDeclTyped d => d.Arg1,
                Zig.ConstDeclMods d => d.Arg1,
                Zig.ConstDeclTypedMods d => d.Arg1,
                Zig.VarDecl d => d.Arg1,
                Zig.VarDeclTyped d => d.Arg1,
                Zig.VarDeclMods d => d.Arg1,
                Zig.VarDeclTypedMods d => d.Arg1,
                Zig.VarDeclThreadLocal d => d.Arg2,
                Zig.StructDecl d => d.Arg1,
                Zig.ExternStructDecl d => d.Arg1,
                Zig.PackedStructDecl d => d.Arg1,
                Zig.PackedStructDeclBacked d => d.Arg1,
                Zig.EnumDecl d => d.Arg1,
                Zig.EnumDeclTyped d => d.Arg1,
                Zig.UnionDecl d => d.Arg1,
                Zig.OpaqueDecl d => d.Arg1,
                _ => null,
            };
            if (token is not null) _publicTopLevelDeclarations.Add(Tok(token));
        }
    }
}
