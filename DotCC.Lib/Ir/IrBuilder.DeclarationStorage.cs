#nullable enable
using System;
using LALR.CC.LexicalGrammar;

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    // Storage and alignment describe the declared object, not its base type.
    // Read only the declaration's type spine: never inspect a nested member,
    // parameter, typeof operand, or alignment operand for storage specifiers.
    private static Storage DeclarationStorage(Item type)
    {
        var storage = Storage.None;
        void Add(Item specifier)
        {
            var next = Tok(specifier) switch
            {
                "static" => Storage.Static,
                "extern" => Storage.Extern,
                "auto" => Storage.Auto,
                "register" => Storage.Register,
                _ => throw new IrUnsupportedException("unsupported storage-class specifier"),
            };
            if (storage != Storage.None)
                throw new IrUnsupportedException("multiple storage-class specifiers in declaration");
            storage = next;
        }
        while (true)
        {
            switch (type.Content)
            {
                case C.TypeStoragePre s: Add(s.Arg0); type = s.Arg1; break;
                case C.TypeStoragePost s: Add(s.Arg1); type = s.Arg0; break;
                case C.TypeAlignasExpr a: type = a.Arg4; break;
                case C.TypeAlignasType a: type = a.Arg4; break;
                case C.TypeAlignasExprPost a: type = a.Arg0; break;
                case C.TypeAlignasTypePost a: type = a.Arg0; break;
                case C.TypeConstPre q: type = q.Arg1; break;
                case C.TypeConstPost q: type = q.Arg0; break;
                case C.TypeVolatile q: type = q.Arg1; break;
                case C.TypeVolatilePost q: type = q.Arg0; break;
                case C.TypeAtomic q: type = q.Arg1; break;
                case C.TypeGnuAttributes a: type = a.Arg0; break;
                case C.TypeAttributedSpecifierRun a: type = a.Arg0; break;
                case C.TypeAttributedSpecifierName a: type = a.Arg0; break;
                case C.TypePtr p: type = p.Arg0; break;
                case C.TypePtrQualConst p: type = p.Arg0; break;
                case C.TypePtrQualVolatile p: type = p.Arg0; break;
                case C.TypePtrQualRestrict p: type = p.Arg0; break;
                default: return storage;
            }
        }
    }

    private FnSig ApplyOrderedFunctionStorage(Item signature, FnSig result)
    {
        Item? type = signature.Content switch
        {
            C.FnSig s => s.Arg0,
            C.FnSigNoArgs s => s.Arg0,
            C.FnSigVoidArgs s => s.Arg0,
            C.FnSigParen s => s.Arg0,
            C.FnSigParenNoArgs s => s.Arg0,
            C.FnSigParenVoidArgs s => s.Arg0,
            C.FnSigRetFnPtr s => s.Arg0,
            C.FnSigRetFnPtrNoArgs s => s.Arg0,
            C.FnSigRetFnPtrVoid s => s.Arg0,
            _ => null,
        };
        return type is { } declarationType ? DeclarationStorage(declarationType) switch
        {
            Storage.Static => result with { IsStatic = true },
            Storage.Auto or Storage.Register => throw new IrUnsupportedException("automatic storage on a function declaration"),
            _ => result,
        } : result;
    }

    private enum OrderedDeclarationKind { List, Array, Aggregate, Designated, String, Callback, PointerToArray, Empty, EmptyArray }
    private readonly record struct OrderedDeclaration(Item Type, Item Name, OrderedDeclarationKind Kind,
        Item? Bounds = null, Item? Initializer = null, bool InferOuter = false, bool Wide = false, bool ScalarCallback = false);

    // Share the existing declaration builders for every spelling of the
    // specifier sequence. The parser still supplies the original declarator,
    // bounds, initializer and positions; no source or token rewriting is used.
    private OrderedDeclaration? OrderedObjectDeclaration(Item declaration) => declaration.Content switch
    {
        C.GlobalDeclList d => new(d.Arg0, d.Arg1, OrderedDeclarationKind.List),
        C.Decl d => new(d.Arg0, d.Arg1, OrderedDeclarationKind.List),
        C.GlobalArr d => new(d.Arg0, d.Arg1, OrderedDeclarationKind.Array, d.Arg2),
        C.GlobalArrIncomplete d => new(d.Arg0, d.Arg1, OrderedDeclarationKind.Array),
        C.GlobalArrInit d => new(d.Arg0, d.Arg1, OrderedDeclarationKind.Array, d.Arg2, d.Arg5),
        C.GlobalArrInitImplicit d => new(d.Arg0, d.Arg1, OrderedDeclarationKind.Array, Initializer: d.Arg6),
        C.GlobalArrInitOuterImplicit d => new(d.Arg0, d.Arg1, OrderedDeclarationKind.Array, d.Arg4, d.Arg7, InferOuter: true),
        C.DeclArr d => new(d.Arg0, d.Arg1, OrderedDeclarationKind.Array, d.Arg2),
        C.DeclArrInit d => new(d.Arg0, d.Arg1, OrderedDeclarationKind.Array, d.Arg2, d.Arg5),
        C.DeclArrInitImplicit d => new(d.Arg0, d.Arg1, OrderedDeclarationKind.Array, Initializer: d.Arg6),
        C.DeclArrInitOuterImplicit d => new(d.Arg0, d.Arg1, OrderedDeclarationKind.Array, d.Arg4, d.Arg7, InferOuter: true),
        C.DeclArrEmptyInit d => new(d.Arg0, d.Arg1, OrderedDeclarationKind.EmptyArray, d.Arg2),
        C.GlobalStructInit d => new(d.Arg0, d.Arg1, OrderedDeclarationKind.Aggregate, Initializer: d.Arg4),
        C.GlobalStructDesignated d => new(d.Arg0, d.Arg1, OrderedDeclarationKind.Designated, Initializer: d.Arg4),
        C.DeclStructInit d => new(d.Arg0, d.Arg1, OrderedDeclarationKind.Aggregate, Initializer: d.Arg4),
        C.DeclStructDesignated d => new(d.Arg0, d.Arg1, OrderedDeclarationKind.Designated, Initializer: d.Arg4),
        C.DeclEmptyInit d => new(d.Arg0, d.Arg1, OrderedDeclarationKind.Empty),
        C.GlobalCharArrStr d => new(d.Arg0, d.Arg1, OrderedDeclarationKind.String, Initializer: d.Arg5),
        C.GlobalCharArrStrSized d => new(d.Arg0, d.Arg1, OrderedDeclarationKind.String, d.Arg2, d.Arg4),
        C.DeclCharArrStr d => new(d.Arg0, d.Arg1, OrderedDeclarationKind.String, Initializer: d.Arg5),
        C.DeclCharArrStrSized d => new(d.Arg0, d.Arg1, OrderedDeclarationKind.String, d.Arg2, d.Arg4),
        C.GlobalU16CharArrStr d => new(d.Arg0, d.Arg1, OrderedDeclarationKind.String, Initializer: d.Arg5, Wide: true),
        C.GlobalU16CharArrStrSized d => new(d.Arg0, d.Arg1, OrderedDeclarationKind.String, d.Arg2, d.Arg4, Wide: true),
        C.DeclU16CharArrStr d => new(d.Arg0, d.Arg1, OrderedDeclarationKind.String, Initializer: d.Arg5, Wide: true),
        C.DeclU16CharArrStrSized d => new(d.Arg0, d.Arg1, OrderedDeclarationKind.String, d.Arg2, d.Arg4, Wide: true),
        C.GlobalWCharArrStr d => new(d.Arg0, d.Arg1, OrderedDeclarationKind.String, Initializer: d.Arg5, Wide: true),
        C.GlobalWCharArrStrSized d => new(d.Arg0, d.Arg1, OrderedDeclarationKind.String, d.Arg2, d.Arg4, Wide: true),
        C.DeclWCharArrStr d => new(d.Arg0, d.Arg1, OrderedDeclarationKind.String, Initializer: d.Arg5, Wide: true),
        C.DeclWCharArrStrSized d => new(d.Arg0, d.Arg1, OrderedDeclarationKind.String, d.Arg2, d.Arg4, Wide: true),
        C.GlobalU32CharArrStr d => new(d.Arg0, d.Arg1, OrderedDeclarationKind.String, Initializer: d.Arg5, Wide: true),
        C.GlobalU32CharArrStrSized d => new(d.Arg0, d.Arg1, OrderedDeclarationKind.String, d.Arg2, d.Arg4, Wide: true),
        C.DeclU32CharArrStr d => new(d.Arg0, d.Arg1, OrderedDeclarationKind.String, Initializer: d.Arg5, Wide: true),
        C.DeclU32CharArrStrSized d => new(d.Arg0, d.Arg1, OrderedDeclarationKind.String, d.Arg2, d.Arg4, Wide: true),
        C.GlobalU8CharArrStr d => new(d.Arg0, d.Arg1, OrderedDeclarationKind.String, Initializer: d.Arg5, Wide: true),
        C.GlobalU8CharArrStrSized d => new(d.Arg0, d.Arg1, OrderedDeclarationKind.String, d.Arg2, d.Arg4, Wide: true),
        C.DeclU8CharArrStr d => new(d.Arg0, d.Arg1, OrderedDeclarationKind.String, Initializer: d.Arg5, Wide: true),
        C.DeclU8CharArrStrSized d => new(d.Arg0, d.Arg1, OrderedDeclarationKind.String, d.Arg2, d.Arg4, Wide: true),
        C.GlobalFnPtrArray d => OrderedCallback(d.Arg0),
        C.GlobalFnPtrArrayInit d => OrderedCallback(d.Arg0, d.Arg3),
        C.GlobalFnPtrScalarInit d => OrderedCallback(d.Arg0, d.Arg2, scalar: true),
        C.DeclCallback d => OrderedCallback(d.Arg0),
        C.DeclCallbackInit d => OrderedCallback(d.Arg0, d.Arg2, scalar: true),
        C.DeclCallbackArrayInit d => OrderedCallback(d.Arg0, d.Arg3),
        C.DeclPtrToArr d => new(d.Arg0, d.Arg3, OrderedDeclarationKind.PointerToArray, d.Arg5),
        C.DeclPtrToArrInit d => new(d.Arg0, d.Arg3, OrderedDeclarationKind.PointerToArray, d.Arg5, d.Arg7),
        _ => null,
    };

    private static OrderedDeclaration OrderedCallback(Item declarator, Item? initializer = null, bool scalar = false)
    {
        var type = declarator.Content switch
        {
            C.FnPtrArrayDeclarator d => d.Arg0,
            C.FnPtrArrayDeclaratorNoArgs d => d.Arg0,
            _ => throw new IrUnsupportedException("unsupported callback declaration"),
        };
        return new(type, declarator, OrderedDeclarationKind.Callback, Initializer: initializer, ScalarCallback: scalar);
    }

    private static Item OrderedInitializer(OrderedDeclaration declaration)
        => declaration.Initializer ?? throw new IrUnsupportedException("missing declaration initializer");

    private bool TryBuildOrderedGlobalDeclaration(Item declaration)
    {
        if (OrderedObjectDeclaration(declaration) is not { } d) return false;
        var storage = DeclarationStorage(d.Type);
        if (storage == Storage.None) return false;
        if (storage is Storage.Auto or Storage.Register)
            throw new IrUnsupportedException("automatic storage on a file-scope declaration");
        _declaringInternalGlobal |= storage == Storage.Static;
        switch (d.Kind)
        {
            case OrderedDeclarationKind.List:
                BuildGlobalDecls(d.Type, d.Name, storage, isStatic: storage == Storage.Static); break;
            case OrderedDeclarationKind.Array when storage == Storage.Extern && d.Initializer is null:
                DeclareOrderedExternArray(d); break;
            case OrderedDeclarationKind.Array:
                BuildGlobalArr(d.Type, d.Name, d.Bounds, d.Initializer, null, inferOuter: d.InferOuter); break;
            case OrderedDeclarationKind.Aggregate:
                BuildGlobalStructInit(d.Type, d.Name, OrderedInitializer(d)); break;
            case OrderedDeclarationKind.Designated:
                BuildGlobalStructDesignated(d.Type, d.Name, OrderedInitializer(d)); break;
            case OrderedDeclarationKind.String:
                BuildGlobalCharArr(d.Type, d.Name, OrderedInitializer(d), d.Bounds, null, d.Wide); break;
            case OrderedDeclarationKind.Callback:
                BuildGlobalFnPtrArray(d.Name, d.Initializer, scalarInitializer: d.ScalarCallback,
                    isExtern: storage == Storage.Extern && d.Initializer is null); break;
            default: throw new IrUnsupportedException("unsupported storage-qualified global declaration");
        }
        return true;
    }

    private CStmt? BuildOrderedLocalDeclaration(Item declaration)
    {
        if (OrderedObjectDeclaration(declaration) is not { } d) return null;
        var storage = DeclarationStorage(d.Type);
        if (storage is Storage.None or Storage.Auto or Storage.Register) return null;
        if (storage == Storage.Extern)
        {
            if (d.Initializer is not null)
                throw new IrUnsupportedException("block-scope extern object cannot have an initializer");
            if (d.Kind == OrderedDeclarationKind.List) return BuildBlockExternObjects(d.Type, d.Name);
            if (d.Kind == OrderedDeclarationKind.Array)
            {
                DeclareOrderedExternArray(d, blockScope: true);
                return new DeclStmt(Array.Empty<LocalDecl>());
            }
            throw new IrUnsupportedException("unsupported block-scope extern declarator");
        }
        return d.Kind switch
        {
            OrderedDeclarationKind.List => BuildStaticLocalDecls(d.Type, d.Name),
            OrderedDeclarationKind.Array => BuildStaticLocalArr(d.Type, d.Name, d.Bounds, d.Initializer, d.InferOuter),
            OrderedDeclarationKind.Aggregate => BuildStmtStaticStructInit(d.Type, d.Name, OrderedInitializer(d), designated: false),
            OrderedDeclarationKind.Designated => BuildStmtStaticStructInit(d.Type, d.Name, OrderedInitializer(d), designated: true),
            OrderedDeclarationKind.String => BuildStaticLocalCharArr(d.Type, d.Name, OrderedInitializer(d), d.Bounds, d.Wide),
            OrderedDeclarationKind.Callback => BuildOrderedStaticCallback(d),
            OrderedDeclarationKind.PointerToArray => BuildOrderedStaticRowPointer(d),
            OrderedDeclarationKind.Empty => BuildOrderedEmptyStatic(d),
            OrderedDeclarationKind.EmptyArray => Gated(2023, "empty initializer", declaration,
                BuildStaticLocalArr(d.Type, d.Name, d.Bounds, null)),
            _ => throw new IrUnsupportedException("unsupported storage-qualified local declaration"),
        };
    }

    private void DeclareOrderedExternArray(OrderedDeclaration d, bool blockScope = false)
    {
        _sawThreadLocalSpec = false;
        var element = blockScope ? ResolveStaticLocalType(d.Type) : ResolveType(d.Type);
        var dimensions = d.Bounds is { } bounds ? TryConstDims(bounds)
            ?? throw new IrUnsupportedException("extern array requires constant bounds") : null;
        var type = dimensions is { Count: > 0 } ? MakeArrayType(element, dimensions) : new CType.Array(element, null);
        var candidate = new Symbol { Name = Tok(d.Name), Kind = SymKind.Var, Type = type,
            Storage = Storage.Extern, IsGlobal = true, IsThreadLocal = _sawThreadLocalSpec,
            Alignment = DeclarationAlignment(d.Type, type), TargetName = _symbols.Escape(Tok(d.Name)) };
        var key = ScalarGlobalKey(candidate.Name, Storage.Extern);
        if (blockScope && !_scalarGlobals.ContainsKey(key))
        {
            NameRegisteredGlobal(candidate, key);
            _scalarGlobals.Add(key, new ScalarGlobalDeclaration(candidate));
            _symbols.DeclareAlias(candidate);
        }
        else RegisterScalarGlobal(candidate, SrcPos.From(d.Name));
    }

    private CStmt BuildOrderedStaticCallback(OrderedDeclaration d)
    {
        _sawThreadLocalSpec = false;
        var (returnType, name, callback) = CallbackDeclarator(d.Name, staticLocal: true);
        var threadLocal = _sawThreadLocalSpec;
        if (name.Content is not C.FnPtrScalarName scalar)
        {
            var (identifier, dimensions, inferOuter) = name.Content switch
            {
                C.FnPtrArrayNameImplicit a => (a.Arg0, (Item?)null, false),
                C.FnPtrArrayNameImplicitOuter a => (a.Arg0, (Item?)a.Arg3, true),
                C.FnPtrArrayNameSized a => (a.Arg0, (Item?)a.Arg1, false),
                _ => throw new IrUnsupportedException("unsupported static callback array declarator"),
            };
            var target = $"{_symbols.Escape(Tok(identifier))}__s{_staticLocalSeq++}";
            BuildGlobalArr(callback, identifier, dimensions, d.Initializer, target,
                alignment: DeclarationAlignment(returnType, callback), inferOuter: inferOuter, threadLocal: threadLocal);
            return new DeclStmt(Array.Empty<LocalDecl>());
        }
        var symbol = new Symbol { Name = Tok(scalar.Arg0), Kind = SymKind.Var, Type = callback,
            Storage = Storage.Static, IsGlobal = true, IsThreadLocal = threadLocal,
            Alignment = DeclarationAlignment(returnType, callback),
            TargetName = $"{_symbols.Escape(Tok(scalar.Arg0))}__s{_staticLocalSeq++}" };
        _symbols.DeclareAlias(symbol);
        RegisterStaticLocal(symbol, d.Initializer is { } initializer ? BuildExpr(initializer) : null);
        return new DeclStmt(Array.Empty<LocalDecl>());
    }

    private CStmt BuildOrderedStaticRowPointer(OrderedDeclaration d)
    {
        var (name, qualifiers) = QualifiedCallbackName(d.Name);
        if (name.Content is not C.FnPtrScalarName scalar)
            throw new IrUnsupportedException("pointer-to-array declaration requires a scalar name");
        var element = ResolveStaticLocalType(d.Type);
        var dimensions = TryConstDims(d.Bounds ?? throw new IrUnsupportedException("missing row bounds"))
            ?? throw new IrUnsupportedException("pointer-to-array needs constant dimensions");
        var type = new CType.Pointer(MakeArrayType(element, dimensions)).WithQuals(qualifiers);
        var symbol = new Symbol { Name = Tok(scalar.Arg0), Kind = SymKind.Var, Type = type,
            Storage = Storage.Static, IsGlobal = true, IsThreadLocal = _sawThreadLocalSpec,
            Alignment = DeclarationAlignment(d.Type, type), TargetName = $"{_symbols.Escape(Tok(scalar.Arg0))}__s{_staticLocalSeq++}" };
        _symbols.DeclareAlias(symbol);
        RegisterStaticLocal(symbol, d.Initializer is { } initializer ? BuildExpr(initializer) : null);
        return new DeclStmt(Array.Empty<LocalDecl>());
    }

    private CStmt BuildOrderedEmptyStatic(OrderedDeclaration d)
    {
        Gate(2023, "empty initializer", d.Name);
        var type = ResolveStaticLocalType(d.Type);
        var symbol = new Symbol { Name = Tok(d.Name), Kind = SymKind.Var, Type = type,
            Storage = Storage.Static, IsGlobal = true, IsThreadLocal = _sawThreadLocalSpec,
            Alignment = DeclarationAlignment(d.Type, type), TargetName = $"{_symbols.Escape(Tok(d.Name))}__s{_staticLocalSeq++}" };
        RegisterStaticLocal(symbol, new DefaultLit { Type = type });
        return new DeclStmt(Array.Empty<LocalDecl>());
    }
}
