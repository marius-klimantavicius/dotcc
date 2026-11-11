using System.Runtime.InteropServices;
using Cui.Managed;
using Raw = Managed.Cui.Cui;

internal static unsafe partial class Program
{
    private static readonly Dictionary<nuint, ulong> tracked = new();
    private static delegate*<ulong, void*, void*> originalAlloc;
    private static delegate*<void*, ulong, void*, void*> originalRealloc;
    private static delegate*<void*, void*, void> originalFree;
    private static ulong outstanding, highWater;
    private static void* Allocate(ulong size, void* user)
    {
        var result = originalAlloc(size, user);
        if (result != null) { tracked[(nuint)result] = size; outstanding += size; highWater = Math.Max(highWater, outstanding); }
        return result;
    }
    private static void* Reallocate(void* address, ulong size, void* user)
    {
        var result = originalRealloc(address, size, user);
        if (result != null || size == 0)
        {
            if (tracked.Remove((nuint)address, out ulong old)) outstanding -= old;
            if (result != null) { tracked[(nuint)result] = size; outstanding += size; highWater = Math.Max(highWater, outstanding); }
        }
        return result;
    }
    private static void Free(void* address, void* user)
    {
        if (tracked.Remove((nuint)address, out ulong size)) outstanding -= size;
        originalFree(address, user);
    }
    private static void AllocatorCleanup()
    {
        // Instrument actual CUI allocator calls separately from timed/managed
        // allocation samples: the tracking dictionary itself can allocate.
        for (int cycle = 0; cycle < 8; cycle++)
        {
            var surface = new OffscreenSurface(640, 480);
            TrackAllocations(surface.Ui);
            using var edit = new TextBuffer(128, "owned lifecycle");
            for (int frame = 0; frame < 80; frame++)
            {
                using var token = surface.BeginFrame();
                using var column = surface.Ui.Column();
                surface.Ui.Heading("Lifecycle", 2); surface.Ui.Input("edit", edit);
                for (int row = 0; row < 12; row++) { using var id = surface.Ui.PushIdInt(row); surface.Ui.Button("Continue"); }
            }
            ulong retained = outstanding;
            surface.Dispose();
            Check(outstanding == 0 && tracked.Count == 0, "all observed core allocations freed during destruction");
            Console.WriteLine($"ALLOCATOR\tcycle={cycle}\tretained={retained}\thighWater={highWater}\tafterDispose={outstanding}");
        }
        WeakReference<Ui> weak = DisposedCallbackOwner();
        Pressure(); Check(!weak.TryGetTarget(out _), "callback roots released at context destruction");
    }
    private static void TrackAllocations(Ui ui)
    {
        Check(outstanding == 0 && tracked.Count == 0, "allocation tracker starts empty");
        var context = Context(ui);
        originalAlloc = context->alloc.alloc; originalRealloc = context->alloc.realloc; originalFree = context->alloc.free;
        context->alloc.alloc = &Allocate; context->alloc.realloc = &Reallocate; context->alloc.free = &Free;
        highWater = outstanding = 0;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static WeakReference<Ui> DisposedCallbackOwner()
    {
        using var surface = new OffscreenSurface(100, 100);
        surface.Ui.CreateCanvasPainter(_ => { });
        using (surface.BeginFrame())
        {
            var painter = surface.Ui.CreateCanvasPainter(p => p.DrawCircle(new Vec2(10, 10), 4, Color.Accent));
            surface.Ui.Canvas("root", Size.Px(20), Size.Px(20), painter);
        }
        return new(surface.Ui);
    }
}
