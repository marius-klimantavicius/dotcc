using System.Runtime.InteropServices;
using VM = Managed.Interpreters.QuickJs;

namespace Managed.Interpreters;

/// <summary>Owns a QuickJS realm and all outstanding values created in that realm.</summary>
public sealed unsafe class QuickJsContext : IDisposable
{
    private const int MaximumCallbackArity = 256;
    private const int MaximumCallbackArguments = 4096;
    private const int Cesu8Encoding = 1;

    internal readonly QuickJsRuntime Runtime;
    internal VM.JSContext* Pointer;
    private readonly HashSet<QuickJsValue> values = [];
    private readonly List<Func<double[], double>> callbacks = [];
    private GCHandle handle;

    internal QuickJsContext(QuickJsRuntime runtime, VM.JSContext* pointer)
    {
        Runtime = runtime; Pointer = pointer;
        handle = GCHandle.Alloc(new WeakReference<QuickJsContext>(this));
        VM.JS_SetContextOpaque(pointer, (void*)GCHandle.ToIntPtr(handle));
    }

    internal void Check() => ObjectDisposedException.ThrowIf(Pointer == null, this);
    internal QuickJsValue Own(VM.JSValue value)
    {
        if (value.tag == VM.JS_TAG_EXCEPTION) throw TakeException(Pointer);
        try
        {
            var result = new QuickJsValue(this, value);
            values.Add(result);
            return result;
        }
        catch { VM.JS_FreeValue(Pointer, value); throw; }
    }

    public QuickJsValue CreateString(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Runtime.Enter();
        try
        {
            Check();
            byte[] bytes = QuickJsText.Encode(value);
            fixed (byte* text = bytes) return Own(VM.JS_NewStringLen(Pointer, text, (ulong)(bytes.Length - 1)));
        }
        finally { Runtime.Leave(); }
    }

    /// <summary>Copies input bytes into an engine-owned ArrayBuffer.</summary>
    public QuickJsValue CreateArrayBuffer(ReadOnlySpan<byte> bytes)
    {
        Runtime.Enter();
        try
        {
            Check();
            fixed (byte* pointer = bytes) return Own(VM.JS_NewArrayBufferCopy(Pointer, pointer, (ulong)bytes.Length));
        }
        finally { Runtime.Leave(); }
    }

    public QuickJsValue Evaluate(string source, string fileName = "<eval>", bool module = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(fileName);
        QuickJsText.CheckName(fileName);
        Runtime.Enter(cancellationToken);
        try
        {
            Check();
            byte[] input = QuickJsText.Encode(source), name = QuickJsText.Encode(fileName);
            fixed (byte* bytes = input)
            fixed (byte* path = name)
                return Own(VM.JS_Eval(Pointer, (byte*)bytes, (ulong)(input.Length - 1), (byte*)path, module ? VM.JS_EVAL_TYPE_MODULE : VM.JS_EVAL_TYPE_GLOBAL));
        }
        finally { Runtime.Leave(); }
    }

    public QuickJsValue GetGlobal(string name)
    {
        QuickJsText.CheckName(name);
        Runtime.Enter();
        try
        {
            Check();
            var global = VM.JS_GetGlobalObject(Pointer);
            try { fixed (byte* key = QuickJsText.Encode(name)) return Own(VM.JS_GetPropertyStr(Pointer, global, (byte*)key)); }
            finally { VM.JS_FreeValue(Pointer, global); }
        }
        finally { Runtime.Leave(); }
    }

    /// <summary>Copies the value into the global object; the caller retains ownership.</summary>
    public void SetGlobal(string name, QuickJsValue value)
    {
        QuickJsText.CheckName(name);
        ArgumentNullException.ThrowIfNull(value);
        Runtime.Enter();
        try
        {
            Check(); value.Check(this);
            var global = VM.JS_GetGlobalObject(Pointer);
            try
            {
                fixed (byte* key = QuickJsText.Encode(name))
                    if (VM.JS_SetPropertyStr(Pointer, global, (byte*)key, VM.JS_DupValue(Pointer, value.Raw)) < 0)
                        throw TakeException(Pointer);
            }
            finally { VM.JS_FreeValue(Pointer, global); }
        }
        finally { Runtime.Leave(); }
    }

    /// <summary>Registers a numeric callback. Managed exceptions become catchable JS errors.</summary>
    public void RegisterFunction(string name, Func<double[], double> function, int arity = 1)
    {
        ArgumentNullException.ThrowIfNull(function);
        QuickJsText.CheckName(name);
        if (arity < 0 || arity > MaximumCallbackArity) throw new ArgumentOutOfRangeException(nameof(arity));
        Runtime.Enter();
        try
        {
            Check();
            if (callbacks.Count >= short.MaxValue) throw new InvalidOperationException("Callback registration limit exceeded.");
            int id = callbacks.Count;
            callbacks.Add(function);
            fixed (byte* key = QuickJsText.Encode(name))
            {
                // The generic-magic union member has the canonical translated callback signature.
                var callback = VM.JS_NewCFunctionMagic(Pointer, &Invoke, (byte*)key, arity, VM.JSCFunctionEnum.JS_CFUNC_generic_magic, id);
                if (callback.tag == VM.JS_TAG_EXCEPTION) throw TakeException(Pointer);
                var global = VM.JS_GetGlobalObject(Pointer);
                try
                {
                    if (VM.JS_SetPropertyStr(Pointer, global, (byte*)key, callback) < 0) throw TakeException(Pointer);
                }
                finally { VM.JS_FreeValue(Pointer, global); }
            }
        }
        finally { Runtime.Leave(); }
    }

    private static VM.JSValue Invoke(VM.JSContext* pointer, VM.JSValue self, int count, VM.JSValue* arguments, int magic)
    {
        try
        {
            var weak = (WeakReference<QuickJsContext>)GCHandle.FromIntPtr((nint)VM.JS_GetContextOpaque(pointer)).Target!;
            if (!weak.TryGetTarget(out var context)) throw new ObjectDisposedException(nameof(QuickJsContext));
            if (count > MaximumCallbackArguments) throw new ArgumentOutOfRangeException(nameof(count), "Callback argument limit exceeded.");
            var values = new double[count];
            for (int i = 0; i < count; i++)
            {
                double number = 0;
                if (VM.JS_ToFloat64(pointer, &number, arguments[i]) < 0) return new() { tag = VM.JS_TAG_EXCEPTION };
                values[i] = number;
            }
            double result = context.callbacks[magic](values);
            return new() { tag = VM.JS_TAG_FLOAT64, u = new() { float64 = result } };
        }
        catch (Exception error) { return ThrowManaged(pointer, error); }
    }

    internal static VM.JSValue ThrowManaged(VM.JSContext* context, Exception error)
    {
        VM.JSValue exception = default;
        bool ownsException = false;
        try
        {
            exception = VM.JS_NewError(context);
            if (exception.tag == VM.JS_TAG_EXCEPTION) return exception;
            ownsException = true;
            byte[] message = QuickJsText.Encode(error.Message);
            fixed (byte* text = message)
            fixed (byte* key = "message\0"u8)
            {
                var value = VM.JS_NewStringLen(context, (byte*)text, (ulong)(message.Length - 1));
                if (value.tag == VM.JS_TAG_EXCEPTION) return value;
                if (VM.JS_SetPropertyStr(context, exception, (byte*)key, value) < 0)
                    return new() { tag = VM.JS_TAG_EXCEPTION };
            }
            ownsException = false;
            return VM.JS_Throw(context, exception);
        }
        catch { return VM.JS_ThrowOutOfMemory(context); }
        finally { if (ownsException) VM.JS_FreeValue(context, exception); }
    }

    internal static QuickJsException TakeException(VM.JSContext* context)
    {
        var error = VM.JS_GetException(context);
        try { return new QuickJsException(String(context, error)); }
        finally { VM.JS_FreeValue(context, error); }
    }

    internal static string String(VM.JSContext* context, VM.JSValue value)
    {
        ulong length = 0;
        byte* text = VM.JS_ToCStringLen2(context, &length, value, Cesu8Encoding);
        if (text == null)
        {
            // User-defined toString can throw, including while formatting an
            // exception/rejection. Consume that pending exception once without
            // recursively attempting to format another throwing object.
            var conversionError = VM.JS_GetException(context);
            VM.JS_FreeValue(context, conversionError);
            throw new QuickJsException("QuickJS string conversion failed.");
        }
        try { return QuickJsText.Decode(text, length); }
        finally { VM.JS_FreeCString(context, text); }
    }

    internal void Forget(QuickJsValue value) => values.Remove(value);

    public void Dispose()
    {
        if (Pointer == null) return;
        Runtime.Enter();
        try { Release(); }
        finally { Runtime.Leave(); }
    }

    internal void Release()
    {
        if (Pointer == null) return;
        while (values.Count != 0)
        {
            using var iterator = values.GetEnumerator();
            iterator.MoveNext();
            iterator.Current.Release();
        }
        values.Clear();
        VM.JS_FreeContext(Pointer); Pointer = null;
        // Native functions and jobs can retain their realm after the facade
        // context is disposed. Callback roots belong to the runtime lifetime.
    }

    internal void ReleaseCallbackRoots()
    {
        callbacks.Clear();
        if (handle.IsAllocated) handle.Free();
    }
}

/// <summary>An owning JS value. Context/runtime disposal invalidates outstanding values.</summary>
public sealed unsafe class QuickJsValue : IDisposable
{
    internal readonly QuickJsContext Context;
    internal VM.JSValue Raw;
    private bool disposed;
    internal QuickJsValue(QuickJsContext context, VM.JSValue value) { Context = context; Raw = value; }
    internal void Check(QuickJsContext context)
    {
        Context.Check();
        ObjectDisposedException.ThrowIf(disposed, this);
        if (!ReferenceEquals(Context.Runtime, context.Runtime)) throw new ArgumentException("Values cannot cross QuickJS runtime boundaries.");
    }

    public QuickJsValue Duplicate()
    {
        Context.Runtime.Enter();
        try { Check(Context); return Context.Own(VM.JS_DupValue(Context.Pointer, Raw)); }
        finally { Context.Runtime.Leave(); }
    }

    public QuickJsValue GetProperty(string name)
    {
        QuickJsText.CheckName(name);
        Context.Runtime.Enter();
        try
        {
            Check(Context);
            fixed (byte* key = QuickJsText.Encode(name)) return Context.Own(VM.JS_GetPropertyStr(Context.Pointer, Raw, (byte*)key));
        }
        finally { Context.Runtime.Leave(); }
    }

    public override string ToString()
    {
        Context.Runtime.Enter();
        try { Check(Context); return QuickJsContext.String(Context.Pointer, Raw); }
        finally { Context.Runtime.Leave(); }
    }

    public double ToDouble()
    {
        Context.Runtime.Enter();
        try
        {
            Check(Context); double result = 0;
            if (VM.JS_ToFloat64(Context.Pointer, &result, Raw) < 0) throw QuickJsContext.TakeException(Context.Pointer);
            return result;
        }
        finally { Context.Runtime.Leave(); }
    }

    /// <summary>Copies an ArrayBuffer into managed storage; no engine pointer escapes.</summary>
    public byte[] ToArrayBuffer()
    {
        Context.Runtime.Enter();
        try
        {
            Check(Context); ulong length = 0;
            byte* pointer = VM.JS_GetArrayBuffer(Context.Pointer, &length, Raw);
            if (pointer == null && VM.JS_HasException(Context.Pointer) != 0) throw QuickJsContext.TakeException(Context.Pointer);
            if (length > int.MaxValue) throw new OverflowException("ArrayBuffer exceeds managed array capacity.");
            return new ReadOnlySpan<byte>(pointer, (int)length).ToArray();
        }
        finally { Context.Runtime.Leave(); }
    }

    public void Dispose()
    {
        if (disposed) return;
        Context.Runtime.Enter();
        try { Release(); }
        finally { Context.Runtime.Leave(); }
    }

    internal void Release()
    {
        if (disposed) return;
        disposed = true;
        VM.JS_FreeValue(Context.Pointer, Raw);
        Context.Forget(this);
    }
}
