# ManagedGallery

An authored safe C# gallery using `Cui.Managed` for the same 27-page workload as
the pinned upstream gallery. The project explicitly disables unsafe blocks and
references only the ManagedCui wrapper. The translated original companion is an
independent oracle, not an implementation dependency of this application.

The wrapper uses `Managed.Forms.Cui` internally. The safe `Cui.Managed`
namespace and all project/assembly names are unchanged.

Choose external directories for build output and temporary files:

```bash
export TMPDIR="$MANAGED_CUI_TEMP"
dotnet build cui/samples/ManagedGallery/ManagedGallery.csproj -c Release \
  --artifacts-path "$MANAGED_CUI_ARTIFACTS"
dotnet "$MANAGED_CUI_ARTIFACTS/bin/ManagedGallery/release/ManagedGallery.dll"
```

Use `--list-pages`, `--page INDEX|NAME`, `--gallery-smoke`, `--capture PATH`, or
`--help`. Captures are raw RGBA. `--gallery-smoke` visits every page; full
differential, interaction and safety qualification use the test suites rather
than treating a smoke run as acceptance.

Build with `-p:CuiVariant=Raw` to select raw translated CUI through the same
ordinary wrapper reference. NativeAOT uses `dotnet publish -r linux-x64
-p:PublishAot=true` (or the actual target RID) with the same external artifacts
path. Windows compilation does not establish Windows execution; it remains unrun.
Desktop rendering uses the committed managed GLFW port and OpenGL. Linux needs
an X11 display; XWayland can supply it. Native Wayland is outside this campaign.

`GalleryDemo` owns the application state, data and edit/terminal resources.
Dispose it before its UI context. Pages are split by component family across
`GalleryPrimary.cs`, `GalleryExtendedA.cs` and `GalleryExtendedB.cs`; the shell,
navigation and theme controls are in `GalleryDemo.cs`. The application uses
typed options, checked scopes and owned buffers, without a raw-context escape.
The original row-position selection behavior through data-grid sorting is
preserved. Existing scalar UTF-8/emoji behavior and adaptations are retained.

The design, content, data and image-generation algorithms derive from CUI
revision `5a9c12b433d4501bc2f5632ead3a1cc05e5b1ee5` (1.4.0). Original sources and provenance
are retained under `generated/TranslatedCuiGallery/upstream`, with CUI's license
and dependency notices in that companion's `notices` directory. The safe wrapper,
translated core, embedded assets and copied GLFW port retain their respective
license boundaries. See the [API guide](../../docs/MANAGED-API-GUIDE.md).
