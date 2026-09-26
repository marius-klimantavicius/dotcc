using System.Runtime.InteropServices;
using VM = Managed.Interpreters.Pinta;

internal static unsafe class ResolverProbes
{
    private sealed class State(byte[] bytes, bool earlyEof)
    {
        internal readonly byte[] Bytes = bytes;
        internal readonly bool EarlyEof = earlyEof;
        internal int Position, Opens, Reads, Closes;
    }
    internal static bool Run(byte[] bytes, bool earlyEof)
    {
        var state = new State(bytes, earlyEof);
        var context = GCHandle.Alloc(state);
        void* arena = NativeMemory.AllocZeroed(4 * 1024 * 1024);
        try
        {
            VM.PintaApiEnvironment environment = new()
            {
                environment_context = (void*)GCHandle.ToIntPtr(context),
                memory = arena, memory_length = 4 * 1024 * 1024,
                heap_length = 1024 * 1024, stack_length = 64 * 1024,
                platform_encoding = (VM.PintaApiEncoding)2,
                file_open = &Open, file_size = &Size, file_read = &Read, file_close = &Close
            };
            var api = VM.pinta_api_create(&environment);
            if (api == null) return false;
            fixed (char* key = "module")
            {
                VM.PintaApiString name = new() { string_data = key, string_length = 6, string_encoding = (VM.PintaApiEncoding)2 };
                void* module = api->load_module(api, &name);
                return (module != null) != earlyEof && state.Opens == 1 && state.Closes == 1 && state.Reads > 1;
            }
        }
        finally { NativeMemory.Free(arena); context.Free(); }
    }
    private static State Get(void* token) => (State)GCHandle.FromIntPtr((nint)token).Target!;
    private static void* Open(void* token, void* name, uint length)
    {
        var state = Get(token);
        state.Opens++;
        return (void*)1;
    }
    private static uint Size(void* token, void* handle) => (uint)Get(token).Bytes.Length;
    private static uint Read(void* token, void* handle, void* buffer, uint length)
    {
        var state = Get(token);
        state.Reads++;
        if (state.EarlyEof && state.Reads > 2) return 0;
        int count = Math.Min(3, (int)Math.Min(length, (uint)(state.Bytes.Length - state.Position)));
        state.Bytes.AsSpan(state.Position, count).CopyTo(new Span<byte>(buffer, count));
        state.Position += count;
        if (state.Reads == 2) GC.Collect();
        return (uint)count;
    }
    private static void Close(void* token, void* handle) => Get(token).Closes++;
}
