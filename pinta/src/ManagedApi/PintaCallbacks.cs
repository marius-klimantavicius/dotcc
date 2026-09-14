using VM = Managed.Interpreters.Pinta;

namespace Managed.Interpreters;

public delegate void PintaHostFunction(ref PintaCallContext call);

/// <summary>Valid only for the duration of a registered application callback.</summary>
public readonly unsafe ref struct PintaCallContext
{
    private readonly VM.PintaCore* core;
    private readonly VM.PintaReference* arguments;
    private readonly VM.PintaReference* result;
    internal PintaCallContext(VM.PintaCore* core, VM.PintaReference* arguments, VM.PintaReference* result)
    {
        this.core = core;
        this.arguments = arguments;
        this.result = result;
    }

    public uint ArgumentCount { get { EnsureActive(); return VM.pinta_array_ref_get_length(arguments); } }

    public int GetInteger(uint index)
    {
        EnsureActive();
        VM.PintaReference value = default;
        VM.PintaNativeFrame frame = new() { references = &value, length = 1, next = core->native };
        core->native = &frame;
        try
        {
            Check(VM.pinta_lib_array_get_item(core, arguments, index, &value));
            int integer = 0;
            var type = VM.pinta_core_get_type(core, &value);
            Check(type->to_integer_value(core, &value, &integer));
            return integer;
        }
        finally { core->native = frame.next; }
    }

    public string? GetString(uint index)
    {
        EnsureActive();
        VM.PintaReference value = default;
        VM.PintaNativeFrame frame = new() { references = &value, length = 1, next = core->native };
        core->native = &frame;
        try
        {
            Check(VM.pinta_lib_array_get_item(core, arguments, index, &value));
            if (value.reference == null) return null;
            var type = VM.pinta_core_get_type(core, &value);
            Check(type->to_string(core, &value, &value));
            Check(VM.pinta_lib_string_to_string(core, &value, &value));
            return new string((char*)VM.pinta_string_ref_get_data(&value), 0,
                checked((int)VM.pinta_string_ref_get_length(&value)));
        }
        finally { core->native = frame.next; }
    }

    // The caller pinta_code_call_internal keeps both arguments and return_value rooted.
    public void ReturnInteger(int value)
    {
        EnsureActive();
        Check(VM.pinta_lib_integer_alloc_value(core, value, result));
    }
    public void ReturnString(string? value)
    {
        EnsureActive();
        if (value is null) { result->reference = null; return; }
        fixed (char* text = value)
            Check(VM.pinta_lib_string_alloc_copy(core, text, checked((uint)value.Length), result));
    }
    public void Collect(bool compact = true)
    {
        EnsureActive();
        VM.pinta_core_gc(core, (byte)(compact ? 1 : 0));
    }
    private static void Check(VM.PintaException status)
    {
        if ((uint)status != 0) throw new PintaException((uint)status);
    }
    private void EnsureActive()
    {
        if (core == null || arguments == null || result == null)
            throw new InvalidOperationException("A call context is valid only inside an application callback.");
    }
}

public sealed unsafe partial class PintaEngine
{
    private readonly PintaHostFunction?[] functions = new PintaHostFunction?[8];
    private bool functionTableAllocated;

    /// <summary>Registers an internal-call bytecode token (0 through 7). Token 0 replaces upstream output.</summary>
    public void RegisterFunction(uint token, PintaHostFunction function)
    {
        ArgumentNullException.ThrowIfNull(function);
        if (token >= 8) throw new ArgumentOutOfRangeException(nameof(token));
        lock (gate)
        {
            Enter();
            try
            {
                var core = (VM.PintaCore*)api->core;
                if (!functionTableAllocated)
                {
                    var table = (delegate*<VM.PintaCore*, VM.PintaReference*, VM.PintaReference*, VM.PintaException>*)
                        VM.pinta_memory_alloc(core->memory, (uint)(8 * sizeof(nint)));
                    if (table == null) throw new OutOfMemoryException("The arena cannot hold the callback table.");
                    for (int index = 0; index < 8; index++) table[index] = &MissingFunction;
                    table[0] = core->internal_functions[0];
                    core->internal_functions = table;
                    core->internal_functions_length = 8;
                    functionTableAllocated = true;
                }
                core->internal_functions[token] = token switch
                {
                    0 => &Function0, 1 => &Function1, 2 => &Function2, 3 => &Function3,
                    4 => &Function4, 5 => &Function5, 6 => &Function6, _ => &Function7
                };
                functions[token] = function;
            }
            finally { Leave(); }
        }
    }

    private static VM.PintaException InvokeFunction(uint token, VM.PintaCore* core, VM.PintaReference* args, VM.PintaReference* result)
    {
        try
        {
            var env = (VM.PintaApiEnvironment*)core->environment->native_environment;
            var state = State(env->environment_context);
            if (state.Owner is null || !state.Owner.TryGetTarget(out var owner) || owner.functions[token] is not { } function)
                return (VM.PintaException)8;
            var call = new PintaCallContext(core, args, result);
            function(ref call);
            GC.KeepAlive(owner);
            return (VM.PintaException)0;
        }
        catch (PintaException error) { return (VM.PintaException)error.Status; }
        catch { return (VM.PintaException)11; }
    }
    private static VM.PintaException MissingFunction(VM.PintaCore* c, VM.PintaReference* a, VM.PintaReference* r) => (VM.PintaException)8;
    private static VM.PintaException Function0(VM.PintaCore* c, VM.PintaReference* a, VM.PintaReference* r) => InvokeFunction(0,c,a,r);
    private static VM.PintaException Function1(VM.PintaCore* c, VM.PintaReference* a, VM.PintaReference* r) => InvokeFunction(1,c,a,r);
    private static VM.PintaException Function2(VM.PintaCore* c, VM.PintaReference* a, VM.PintaReference* r) => InvokeFunction(2,c,a,r);
    private static VM.PintaException Function3(VM.PintaCore* c, VM.PintaReference* a, VM.PintaReference* r) => InvokeFunction(3,c,a,r);
    private static VM.PintaException Function4(VM.PintaCore* c, VM.PintaReference* a, VM.PintaReference* r) => InvokeFunction(4,c,a,r);
    private static VM.PintaException Function5(VM.PintaCore* c, VM.PintaReference* a, VM.PintaReference* r) => InvokeFunction(5,c,a,r);
    private static VM.PintaException Function6(VM.PintaCore* c, VM.PintaReference* a, VM.PintaReference* r) => InvokeFunction(6,c,a,r);
    private static VM.PintaException Function7(VM.PintaCore* c, VM.PintaReference* a, VM.PintaReference* r) => InvokeFunction(7,c,a,r);
}
