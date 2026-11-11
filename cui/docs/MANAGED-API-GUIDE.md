# Using ManagedCui

Reference `cui/src/ManagedCui/ManagedCui.csproj` and import `Cui.Managed`.
The library targets .NET 10. Caller projects can set `AllowUnsafeBlocks=false`.
The default project reference selects processed CUI; `-p:CuiVariant=Raw` selects
the raw translation. Neither product needs a native CUI or GLFW library.

## Desktop application

```csharp
using Cui.Managed;

using var app = new Application(new ApplicationOptions
{
    Title = "Workspace", Width = 800, Height = 600,
    Theme = ThemeMode.Light, Accent = Accent.Violet
});
using var name = new TextBuffer(128, "Maya");
bool notifications = true;
while (app.NextFrame())
{
    try
    {
        using var panel = app.Ui.Column(new Box { Pad = 24, Gap = 12 });
        app.Ui.Heading("Workspace", 2);
        app.Ui.Input("name", name, new InputOpts { Label = "Name" });
        app.Ui.Checkbox("Email notifications", ref notifications);
        if (app.Ui.Button("Save", new ButtonOpts { Variant = ButtonVariant.Primary }))
            app.Ui.Toast("Saved");
    }
    finally { app.Render(); }
}
```

`Application.Run(Action<Ui>)` supplies the same loop and exception cleanup.
Register its callback once. `NextFrame` finishes an unrendered previous frame;
calling `Render` explicitly does not cause duplicate presentation on the next
iteration. `CapturePixels` returns an allocated RGBA copy and can re-render for
OpenGL readback. Captures are diagnostic work, not an allocation-free hot path.
Call `Quit` to request loop exit. Use and dispose an application on its creating
thread. CUI supports one active desktop application. The copied managed GLFW
port selects Windows or Linux X11 at runtime; desktop rendering uses OpenGL.

## Frames, scopes and state

Build widgets only in an active frame. Queries such as theme, time and copied
rectangles can be read outside a frame. Create IDs with `MakeId` inside a frame;
the safe `Id` value remains tied to that context. `UiScope` tokens close containers
in reverse order, reject stale generations, tolerate copies/repeated disposal,
and are unwound by frame completion when construction throws. A conditional
container's `IsOpen` determines whether its content should be built:

```csharp
using var menu = ui.Menu("Actions");
if (menu.IsOpen)
{
    if (ui.MenuItem("Archive")) ui.Toast("Archived");
}
```

Scalar immediate values generally use `ref`. Mutable scalar pointers retained
by CUI options use reusable `State<T>` owners:

```csharp
var sort = new State<TableSort>(new TableSort { Column = 0 }); // create once
TableColumn[] columns = [new() { Title = "Name", Sortable = true }];
// Inside a frame:
using (ui.Table("people", columns, new TableOpts { Sort = sort }))
{
    ui.TableRow(false);
    ui.TableCellText("Maya Okafor");
}
TableSort current = sort.Value;
```

Cache arrays and owners outside the frame loop. Options are small values and
default to upstream behavior. Read-only nested arrays use `ReadOnlyMemory<T>`;
mutable nested arrays use `Memory<T>`. Count-inferred overloads avoid redundant
lengths; explicit counts are checked. State, buffers, IDs, textures and callback
registrations bind to their first context and cannot be reused with another.
Create fresh owners when reopening an application. Owner-thread checks apply
after binding; disposing buffer/resource owners is repeatable.

## Text, resources and callbacks

String overloads encode to a reusable stable frame arena without allocating
temporary managed byte arrays. `Text(ReadOnlySpan<char>)`, `TextUtf8` and
`ButtonUtf8` accept stack-backed input; supplied bytes are copied, and UTF-8
overloads reject malformed input and embedded NUL. `TextBuffer` owns persistent
UTF-8 edit storage, with capacity including the terminator. `SetUtf8` and
`CopyUtf8To(Span<byte>)` avoid string conversion; reading `Text` creates a string.
Editors support the translated UTF-8 scalar behavior, including existing emoji
editing. They do not promise grapheme-cluster editing.

Create textures with `ui.ImageCreate`/`ImageLoad`; use the opaque `Texture` owner
with image/media controls. `TerminalBuffer` owns the ANSI parser's backing text
and lines. Dispose them deterministically. Disposed text/terminal/texture owners
reject reuse while the library keeps storage needed by the current deferred
frame alive until safe retirement. The context also releases its texture owners.

Register a canvas painter once, cache it, and dispose it when done:

```csharp
using var painter = ui.CreateCanvasPainter(paint =>
{
    var r = paint.Rect;
    paint.DrawCircle(new Vec2(r.X + r.W / 2, r.Y + r.H / 2),
        MathF.Min(r.W, r.H) / 3, paint.ColorOf(Color.Accent));
});
// Inside subsequent frames:
ui.Canvas("preview", Size.Px(120), Size.Px(80), painter);
```

Paint runs after layout. Its `PaintContext` capability expires on callback exit,
including copies. Widget/lifecycle reentry is rejected. Callback exceptions are
contained at the translated boundary and rethrown after CUI returns. File list
handlers receive a safe output span and return its written count; `Ui.FsList`
provides the translated local-filesystem listing. Reuse handlers rather than
creating capturing lambdas each frame. Returning `-1` reports an ordinary listing
failure without throwing. Heatmap row/column label memories support null entries
for skipped ticks and upstream tooltip fallback. Callback roots survive until context
destruction, with limits of 1,024 registrations and 1,024 painters. A disposed
painter releases its delegate immediately; its bounded registration slot lasts
until context destruction.

## Offscreen rendering and costs

```csharp
using var surface = new OffscreenSurface(640, 480);
using (surface.BeginFrame(1f / 60))
{
    using var panel = surface.Ui.Column(new Box { Pad = 16 });
    surface.Ui.TextUtf8("Hello from software CUI"u8);
}
surface.Render();
surface.CopyPixelsTo(destination); // caller supplies Span<byte>, width*height*4
```

`Pixels` is a read-only span over managed pixel storage; it contains the last
rendered RGBA output. Old spans remain memory-safe across resize/disposal because
they keep their managed storage alive. Software rasterization is separate from
UI construction and is not desktop GLFW presentation.

`GetDiagnostics` copies wrapper arena reservation/use and owner/registration
counts. Its byte counts exclude engine, renderer, buffers and driver storage.
Frame storage is stable, segmented, reused and bounded by `FrameArenaLimit`
(default 16 MiB). Nested managed scopes are limited to 32, and retained state
owners to 65,536. Registration, first growth, resource creation, string results,
dynamic application formatting and captures can allocate. Warm allocation-
conscious wrapper paths add zero managed allocation in the qualified workloads;
this does not imply that every gallery or translated-core operation allocates
nothing. See [qualification](../tests/managed/README.md) and the retained
[library evidence](MANAGED-LIBRARY-VALIDATION.json) for measured scope and gates.

## API coverage and dependency boundary

[The contract](MANAGED-API-CONTRACT.md) maps every distinct call/type used by the
upstream gallery to its typed wrapper. `Ui.Api.cs` and `Types.cs` are the exact
method/option catalog; `Ui.Overloads.cs` adds count-inferred and scope conveniences.
The authored [ManagedGallery](../samples/ManagedGallery/) provides component
examples across forms, navigation, overlays, tables/grids, charts, readers,
planning, media, files, developer tools and instruments. Its project references
only ManagedCui. The separate translated-gallery companion and ManagedConsumer
remain the independent reference and are not part of the showcase dependency graph.

ManagedCui wraps the canonical translated CUI and the committed managed GLFW
port. CUI algorithms, fonts, decoding and renderers retain their existing
licenses/notices under `generated/TranslatedCui`; gallery content derives from
the pinned CUI gallery with its source/provenance retained separately. This API
guards its embedding contracts; the underlying translation and host still use
unsafe implementation code. Actual Windows execution remains unrun. Linux
qualification, cross-compilation and ABI shims do not establish Windows success.
