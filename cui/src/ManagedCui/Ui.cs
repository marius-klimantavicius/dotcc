using System.Runtime.InteropServices;
using System.Runtime.ExceptionServices;
using System.Text;
using Raw = global::Managed.Forms.Cui;
using static global::Managed.Forms.Cui;

namespace Cui.Managed;

/// <summary>Safe owner-thread view of a CUI context. Its surface/application owns its lifetime.</summary>
public sealed unsafe partial class Ui
{
    private cui_ctx* ctx;
    private readonly int owner = Environment.CurrentManagedThreadId;
    // Stable pointees (UTF-8 and arrays), not synchronous singleton arguments.
    private readonly FrameArena arena;
    private bool inFrame;
    private bool painting;
    private bool fileCallback;
    private long generation;
    private long nextScope;
    private long closedScope;
    private readonly List<(long Id, ScopeKind Kind)> scopes = new List<(long Id, ScopeKind Kind)>();
    private readonly List<IStateLease> states = new List<IStateLease>();
    private readonly List<IStateLease> activeStates = new List<IStateLease>();
    private readonly HashSet<TextBuffer> buffers = new HashSet<TextBuffer>();
    private readonly HashSet<TerminalBuffer> terminals = new HashSet<TerminalBuffer>();
    private readonly Dictionary<ulong, Texture> textures = new Dictionary<ulong, Texture>();
    private readonly List<Texture> pendingTextures = new List<Texture>();
    private readonly List<GCHandle> callbackRoots = new List<GCHandle>();
    private ExceptionDispatchInfo? callbackError;
    private bool arraysDirty;
    internal Ui(cui_ctx* context, int arenaLimit)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        ctx = context; arena = new FrameArena(arenaLimit);
    }
    internal void Check()
    {
        CheckOwnerThread();
        ObjectDisposedException.ThrowIf(ctx == null, this);
    }
    internal void CheckOwnerThread()
    {
        if (Environment.CurrentManagedThreadId != owner) throw new InvalidOperationException("Use CUI on its creating thread.");
    }
    private void CheckFrame()
    {
        Check();
        if (!inFrame || painting || fileCallback) throw new InvalidOperationException("Build widgets inside a frame, outside paint callbacks.");
    }
    private void CheckBegin()
    {
        if (scopes.Count >= 32) throw new InvalidOperationException("CUI managed scope nesting limit exceeded.");
    }
    internal void CheckLifecycle()
    {
        Check();
        if (painting || fileCallback) throw new InvalidOperationException("Do not change CUI lifecycle inside a callback.");
    }
    private void CheckDraw()
    {
        Check();
        if (!inFrame || !painting) throw new InvalidOperationException("Draw inside a canvas paint callback.");
    }
    internal void PrepareFrame()
    {
        CheckLifecycle();
        if (inFrame) throw new InvalidOperationException("Finish the previous frame before starting another.");
    }
    internal UiFrame AdoptFrame()
    {
        // Desktop NextFrame can poll and re-present the previous draw data
        // before native begin. Retire inputs/resources only after it returns a
        // new frame, when native begin has discarded previous build pointers.
        ReleasePendingTextures(); arena.Reset(); ClearArrays(); arraysDirty = false; activeStates.Clear();
        generation++; closedScope = nextScope; scopes.Clear();
        foreach (var buffer in buffers) buffer.Release(); buffers.Clear();
        foreach (var terminal in terminals) terminal.Release(); terminals.Clear();
        inFrame = true; return new UiFrame(this, generation);
    }
    internal UiFrame BeginFrame(float dt)
    {
        if (!float.IsFinite(dt) || dt < 0) throw new ArgumentOutOfRangeException(nameof(dt));
        PrepareFrame(); Raw.cui_begin_frame(ctx, dt); return AdoptFrame();
    }
    internal void FinishFrame(long expected)
    {
        Check();
        if (generation != expected) throw new InvalidOperationException("Frame token is stale.");
        if (!inFrame) return;
        if (painting || fileCallback) throw new InvalidOperationException("Cannot finish a frame from a callback.");
        try
        {
            while (scopes.Count > 0)
            {
                var entry = scopes[^1]; NativeEnd(entry.Kind); scopes.RemoveAt(scopes.Count - 1); closedScope = Math.Max(closedScope, entry.Id);
            }
            Raw.cui_end_frame(ctx);
            SyncState();
        }
        finally { inFrame = false; }
        ThrowCallbackError();
    }
    internal void EndFrame() { Check(); if (inFrame) FinishFrame(generation); }
    internal void DestroyContext(Action destroy)
    {
        CheckLifecycle();
        try { EndFrame(); }
        finally
        {
            // Native destruction can still consult callbacks and edit backing storage.
            try { try { foreach (var texture in textures.Values) Raw.cui_image_destroy(ctx, texture.Value); } finally { destroy(); } }
            finally
            {
                ctx = null;
                foreach (var texture in textures.Values) texture.Invalidate(); textures.Clear();
                foreach (var state in states) state.Dispose(); states.Clear(); activeStates.Clear();
                foreach (var buffer in buffers) buffer.Release(); buffers.Clear();
                foreach (var terminal in terminals) terminal.Release(); terminals.Clear();
                foreach (var painter in painters.Values) painter.Callback = null;
                painters.Clear(); fileRoots.Clear(); selfRoot = 0;
                foreach (var root in callbackRoots) root.Free(); callbackRoots.Clear();
                pendingTextures.Clear(); callbackError = null;
                arena.Dispose(); ClearArrays();
            }
        }
    }
    internal bool IsDisposed => ctx == null;
    internal void Contain(Exception error) => callbackError ??= ExceptionDispatchInfo.Capture(error);
    private void ThrowCallbackError()
    {
        var error = callbackError; callbackError = null; error?.Throw();
    }
    private void SyncState()
    {
        foreach (var lease in activeStates) lease.Read();
        // Retained writable arrays are currently grid selection and reorder
        // order. Text/cell/layout calls cannot mutate them. Synchronize only
        // after registration or the upstream mutation points, not per cell.
        if (arraysDirty) { SyncArrays(); arraysDirty = false; }
        if (!painting) ThrowCallbackError();
    }
    private R* Bind<T, R>(State<T>? state, delegate*<Ui, T, R> encode, delegate*<Ui, R, T, T> decode) where R : unmanaged
    {
        if (state == null) return null;
        if (state.Owner != null && !ReferenceEquals(state.Owner, this)) throw new ArgumentException("State belongs to another context.");
        if (state.Lease == null)
        {
            if (states.Count >= 65536) throw new InvalidOperationException("CUI state owner limit exceeded.");
            state.Owner = this;
            var created = new StateLease<T, R>(this, state, encode, decode); state.Lease = created; states.Add(created);
        }
        var lease = (StateLease<T, R>)state.Lease;
        if (!activeStates.Contains(lease)) { lease.Write(); activeStates.Add(lease); }
        return lease.Pointer;
    }
    private byte* Use(TextBuffer? buffer)
    {
        if (buffer == null) return null;
        if (!buffers.Contains(buffer)) { var pointer = buffer.Acquire(this); buffers.Add(buffer); return pointer; }
        // Repeated Acquire would add a lease; validate without changing ownership.
        _ = buffer.Length;
        return buffer.Pointer;
    }
    private byte* Utf8Required(string value) { ArgumentNullException.ThrowIfNull(value); return Utf8(value); }
    private byte* Utf8(string? value) => value == null ? null : Utf8(value.AsSpan());
    private byte* Utf8(ReadOnlySpan<char> value)
    {
        if (value.Contains('\0')) throw new ArgumentException("CUI strings cannot contain NUL.", nameof(value));
        var size = Encoding.UTF8.GetByteCount(value);
        var pointer = (byte*)arena.Allocate(checked(size + 1));
        Encoding.UTF8.GetBytes(value, new Span<byte>(pointer, size)); pointer[size] = 0; return pointer;
    }
    private byte** Strings(ReadOnlySpan<string> values)
    {
        var result = (byte**)arena.Allocate(checked(values.Length * sizeof(byte*)));
        for (var i = 0; i < values.Length; i++)
        {
            if (values[i] == null) throw new ArgumentException("String array entries must be non-null.", nameof(values));
            result[i] = Utf8(values[i]);
        }
        return result;
    }
    private byte** OptionalStrings(ReadOnlySpan<string?> values)
    {
        var result = (byte**)arena.Allocate(checked(values.Length * sizeof(byte*)));
        for (var i = 0; i < values.Length; i++) result[i] = Utf8(values[i]);
        return result;
    }
    private static string? Decode(byte* value, string? old = null, int maximum = int.MaxValue)
    {
        if (value == null) return null;
        var length = 0; while (length < maximum && value[length] != 0) length++;
        var utf8 = new ReadOnlySpan<byte>(value, length);
        if (old != null)
        {
            var index = 0; var equal = true;
            foreach (var rune in old.EnumerateRunes())
            {
                if (Rune.DecodeFromUtf8(utf8[index..], out var next, out var used) != System.Buffers.OperationStatus.Done || rune != next) { equal = false; break; }
                index += used;
            }
            if (equal && index == length) return old;
        }
        return Encoding.UTF8.GetString(utf8);
    }
    private static void WriteInline(string? value, byte* output, int capacity)
    {
        if (value == null) { output[0] = 0; return; }
        if (value.Contains('\0')) throw new ArgumentException("Text cannot contain NUL.");
        var length = Encoding.UTF8.GetByteCount(value);
        if (length >= capacity) throw new ArgumentException("Text exceeds the upstream field capacity.");
        Encoding.UTF8.GetBytes(value, new Span<byte>(output, length)); output[length] = 0;
    }
    private uint Unwrap(Id id)
    {
        if (id.IsEmpty) return 0;
        if (!ReferenceEquals(id.Owner, this)) throw new ArgumentException("Id belongs to another context.");
        return id.Value;
    }
    private static void CheckLength(int count, int length, string name)
    {
        if (count < 0 || count > length) throw new ArgumentOutOfRangeException(name, "Count exceeds the supplied storage.");
    }
    private static void CheckUtf8Length(string? text, int length)
    {
        if (length < -1 || (length >= 0 && length > Encoding.UTF8.GetByteCount(text ?? ""))) throw new ArgumentOutOfRangeException(nameof(length));
    }
    private UiScope TrackScope(ScopeKind kind, bool open, int result)
    {
        if (!open) return new UiScope(this, 0, generation, false, result);
        var id = ++nextScope; scopes.Add((id, kind)); return new UiScope(this, id, generation, true, result);
    }
    internal void CloseScope(long id, long expected)
    {
        Check();
        if (expected != generation) throw new InvalidOperationException("Scope token is stale.");
        if (id <= closedScope || !inFrame) return;
        var index = scopes.Count - 1;
        while (index >= 0 && scopes[index].Id != id) index--;
        if (index < 0) return;
        if (index != scopes.Count - 1) throw new InvalidOperationException("Dispose CUI scopes in reverse nesting order.");
        NativeEnd(scopes[index].Kind); scopes.RemoveAt(index);
        // Individual closed ids are absent from the stack; never suppress an outer token.
    }
    private void BeforeExplicitEnd(ScopeKind kind)
    {
        if (scopes.Count == 0 || scopes[^1].Kind != kind) throw new InvalidOperationException("End does not match the active CUI scope.");
        scopes.RemoveAt(scopes.Count - 1);
    }
    public UiScope Row(Box opts = default) => RowBegin(opts);
    public UiScope Column(Box opts = default) => ColumnBegin(opts);
    public UiScope Card(CardOpts opts = default) => CardBegin(opts);
    public UiScope Scroll(ScrollOpts opts = default) => ScrollBegin(opts);
    public UiScope Menu(string label, MenuOpts opts = default) => MenuBegin(label, opts);
    public UiScope Popover(string id, PopoverOpts opts = default) => PopoverBegin(id, opts);
    public UiScope ContextMenu(string id) => ContextMenuBegin(id);
    public UiScope Dialog(string title, ref bool open, DialogOpts opts = default) => DialogBegin(title, ref open, opts);
    public UiScope Sheet(string title, ref bool open, SheetOpts opts = default) => SheetBegin(title, ref open, opts);
    public UiScope Table(string id, ReadOnlySpan<TableColumn> columns, TableOpts opts = default) => TableBegin(id, columns, columns.Length, opts);
    public void Text(ReadOnlySpan<char> text, TextOpts opts = default)
    {
        CheckFrame(); var native = ToNative(opts); Raw.cui_text_ex(ctx, Utf8(text), &native);
    }

}
