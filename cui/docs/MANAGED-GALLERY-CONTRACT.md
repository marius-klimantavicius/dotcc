# ManagedGallery implementation contract

The API contract is [MANAGED-API-CONTRACT.md](MANAGED-API-CONTRACT.md).
Author C# against `Cui.Managed`; no unsafe code, raw types, translated-gallery
references or runtime source conversion in the showcase.

All gallery source uses namespace `Cui.ManagedGallery`. The primary file defines
`public sealed partial class GalleryDemo : IDisposable` with `public int Page`
and `public void Frame(Ui ui)`. Its constructor accepts no context. All instance
state starts with upstream initial values, including the extended pages' static
state, but is owned per GalleryDemo instance. The primary implementation owns
page names/icons and the complete shell/sidebar/dialogs.

Shared helpers supplied by the primary implementation:

```csharp
private static void Section(Ui ui, string title, string? description);
private T Own<T>(T resource) where T : IDisposable;
```

`Own` adds persistent edit/terminal/resource owners to a list disposed with the
demo. Use it lazily when resources require a context. Resources constructed by
field initializers need disposal in the corresponding partial cleanup method.
Do not allocate widgets, arrays, delegates or resource registrations each frame
when they can be cached. Dynamic formatting may allocate and must be honest.

The primary file declares optional partial extension methods:

```csharp
partial void DrawExtendedA(Ui ui, int page);
partial void DrawExtendedB(Ui ui, int page);
partial void DisposeExtendedA();
partial void DisposeExtendedB();
```

When `Page >= 9`, the shell pushes the extended page's name as its ID scope,
calls both drawing partials with the **global page number**, then pops the scope.
Each implementation switches only its assigned range. Disposal calls both
cleanup partials and disposes shared owned resources. Primary pages are 0–8;
ExtendedA owns 9–18; ExtendedB owns 19–26. Prefix extension instance fields with
`a`/`b` respectively, and private page methods with `A`/`B`, to prevent conflicts.

File ownership during parallel implementation:

| Owner | Files |
| --- | --- |
| Primary | `samples/ManagedGallery/ManagedGallery.csproj`, `Program.cs`, `GalleryDemo.cs`, `GalleryPrimary.cs` |
| ExtendedA | `samples/ManagedGallery/GalleryExtendedA.cs` |
| ExtendedB | `samples/ManagedGallery/GalleryExtendedB.cs` |
| Coordinator | Library fixes, solution, qualification, evidence and other documentation |

No implementation agent changes another owner's file. Report required API
repairs to the coordinator. No sample builds while other agents write source;
the coordinator builds the complete sample after integration. Temporary inputs
and diagnostics use caller-selected external storage. No generated core edits.

Keep all content, layout, options, ordering and interactions faithful to the
original `gallery_ui.c` / `gallery_more.c`. Data grid selection stays attached
to row position through sorting. Preserve the existing emoji behavior. File-list
fixtures, image generators, random/time data and all other callbacks must be
authored safe C# with the same semantics, not a hidden native/raw helper.
