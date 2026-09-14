using System.Runtime.InteropServices;
using VM = Managed.Interpreters.Pinta;

internal static unsafe class ValueExhaustionProbes
{
    internal static bool Run(int kind)
    {
        void* arena = NativeMemory.AllocZeroed(65536);
        try
        {
            VM.PintaApiEnvironment environment = new()
            { memory = arena, memory_length = 65536, heap_length = 1024, stack_length = 1024 };
            var api = VM.pinta_api_create(&environment);
            if (api == null) return false;
            var core = (VM.PintaCore*)api->core;
            VM.PintaReference* roots = stackalloc VM.PintaReference[128];
            new Span<VM.PintaReference>(roots, 128).Clear();
            VM.PintaNativeFrame frame = new() { references = roots, length = 128, next = core->native };
            core->native = &frame;
            try
            {
                uint status = 0;
                int index = 0;
                for (; index < 127; index++)
                {
                    status = (uint)VM.pinta_lib_integer_alloc_value(core, 1000 + index, &roots[index]);
                    if (status != 0) break;
                }
                if (status != 4 || index == 0 || index == 127) return false;
                fixed (char* text = "ĄŽ")
                    status = (uint)(kind switch
                    {
                        0 => VM.pinta_lib_string_alloc_value(core, text, 2, &roots[127]),
                        1 => VM.pinta_lib_char_alloc_value(core, 'Ą', &roots[127]),
                        _ => VM.pinta_lib_weak_alloc(core, &roots[127])
                    });
                return status == 4 && roots[127].reference == null;
            }
            finally { core->native = frame.next; }
        }
        finally { NativeMemory.Free(arena); }
    }
}
