using System.Runtime.InteropServices;
using VM = Managed.Interpreters.QuickJs;

namespace Managed.Interpreters;

/// <summary>Owns a QuickJS realm and all outstanding values created in that realm.</summary>
public sealed unsafe class QuickJsContext : IDisposable
{
    private const int MaximumCallbackArity = 256;
    private const int MaximumCallbackArguments = 4096;
    private const int Cesu8Encoding = 1;

    private readonly HashSet<QuickJsValue> _values = [];
    private readonly List<Func<double[], double>> _callbacks = [];
    private GCHandle _handle;

    internal readonly QuickJsRuntime Runtime;
    internal VM.JSContext* NativeContext;

    internal QuickJsContext(QuickJsRuntime runtime, VM.JSContext* nativeContext)
    {
        Runtime = runtime;
        NativeContext = nativeContext;

        _handle = GCHandle.Alloc(new WeakReference<QuickJsContext>(this));
        VM.JS_SetContextOpaque(nativeContext, (void*)GCHandle.ToIntPtr(_handle));
    }

    internal void Check() => ObjectDisposedException.ThrowIf(NativeContext == null, this);

    internal QuickJsValue Own(VM.JSValue value)
    {
        if (value.tag == VM.JS_TAG_EXCEPTION)
            throw TakeException(NativeContext);

        try
        {
            var result = new QuickJsValue(this, value);
            _values.Add(result);
            return result;
        }
        catch
        {
            VM.JS_FreeValue(NativeContext, value);
            throw;
        }
    }

    public QuickJsValue CreateString(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Runtime.Enter();

        scoped var valueMarshaller = new QuickJsText.ManagedToUnmanagedIn();
        try
        {
            Check();
            valueMarshaller.FromManaged(value, stackalloc byte[QuickJsText.ManagedToUnmanagedIn.BufferSize]);
            return Own(VM.JS_NewStringLen(NativeContext, valueMarshaller.ToUnmanaged(), (ulong)valueMarshaller.Count));
        }
        finally
        {
            valueMarshaller.Free();
            Runtime.Leave();
        }
    }

    /// <summary>Copies input bytes into an engine-owned ArrayBuffer.</summary>
    public QuickJsValue CreateArrayBuffer(ReadOnlySpan<byte> bytes)
    {
        Runtime.Enter();
        try
        {
            Check();
            fixed (byte* pointer = bytes)
                return Own(VM.JS_NewArrayBufferCopy(NativeContext, pointer, (ulong)bytes.Length));
        }
        finally { Runtime.Leave(); }
    }

    public QuickJsValue Evaluate(string source,
        string fileName = "<eval>",
        bool module = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(fileName);
        QuickJsText.CheckName(fileName);

        Runtime.Enter(cancellationToken);

        scoped var sourceMarshaller = new QuickJsText.ManagedToUnmanagedIn();
        scoped var fileNameMarshaller = new QuickJsText.ManagedToUnmanagedIn();
        try
        {
            Check();

            sourceMarshaller.FromManaged(source, stackalloc byte[QuickJsText.ManagedToUnmanagedIn.BufferSize]);
            fileNameMarshaller.FromManaged(fileName, stackalloc byte[QuickJsText.ManagedToUnmanagedIn.BufferSize]);

            return Own(VM.JS_Eval(NativeContext, sourceMarshaller.ToUnmanaged(), (ulong)sourceMarshaller.Count, fileNameMarshaller.ToUnmanaged(), module ? VM.JS_EVAL_TYPE_MODULE : VM.JS_EVAL_TYPE_GLOBAL));
        }
        finally
        {
            sourceMarshaller.Free();
            fileNameMarshaller.Free();

            Runtime.Leave();
        }
    }

    public QuickJsValue GetGlobal(string name)
    {
        QuickJsText.CheckName(name);
        Runtime.Enter();

        scoped var nameMarshaller = new QuickJsText.ManagedToUnmanagedIn();
        try
        {
            Check();

            var global = VM.JS_GetGlobalObject(NativeContext);
            try
            {
                nameMarshaller.FromManaged(name, stackalloc byte[QuickJsText.ManagedToUnmanagedIn.BufferSize]);
                return Own(VM.JS_GetPropertyStr(NativeContext, global, nameMarshaller.ToUnmanaged()));
            }
            finally
            {

                VM.JS_FreeValue(NativeContext, global);
            }
        }
        finally
        {
            nameMarshaller.Free();
            Runtime.Leave();
        }
    }

    /// <summary>Copies the value into the global object; the caller retains ownership.</summary>
    public void SetGlobal(string name, QuickJsValue value)
    {
        QuickJsText.CheckName(name);
        ArgumentNullException.ThrowIfNull(value);

        Runtime.Enter();

        scoped var nameMarshaller = new QuickJsText.ManagedToUnmanagedIn();
        try
        {
            Check();
            value.Check(this);
            var global = VM.JS_GetGlobalObject(NativeContext);
            try
            {
                nameMarshaller.FromManaged(name, stackalloc byte[QuickJsText.ManagedToUnmanagedIn.BufferSize]);

                if (VM.JS_SetPropertyStr(NativeContext, global, nameMarshaller.ToUnmanaged(), VM.JS_DupValue(NativeContext, value.Raw)) < 0)
                    throw TakeException(NativeContext);
            }
            finally
            {
                VM.JS_FreeValue(NativeContext, global);
            }
        }
        finally
        {
            nameMarshaller.Free();

            Runtime.Leave();
        }
    }

    /// <summary>Registers a numeric callback. Managed exceptions become catchable JS errors.</summary>
    public void RegisterFunction(string name, Func<double[], double> function, int arity = 1)
    {
        ArgumentNullException.ThrowIfNull(function);
        QuickJsText.CheckName(name);
        if (arity < 0 || arity > MaximumCallbackArity)
            throw new ArgumentOutOfRangeException(nameof(arity));

        Runtime.Enter();

        scoped var nameMarshaller = new QuickJsText.ManagedToUnmanagedIn();
        try
        {
            Check();
            if (_callbacks.Count >= short.MaxValue)
                throw new InvalidOperationException("Callback registration limit exceeded.");

            var id = _callbacks.Count;
            _callbacks.Add(function);

            nameMarshaller.FromManaged(name, stackalloc byte[QuickJsText.ManagedToUnmanagedIn.BufferSize]);

            // The generic-magic union member has the canonical translated callback signature.
            var callback = VM.JS_NewCFunctionMagic(NativeContext, &Invoke, nameMarshaller.ToUnmanaged(), arity, VM.JSCFunctionEnum.JS_CFUNC_generic_magic, id);
            if (callback.tag == VM.JS_TAG_EXCEPTION)
                throw TakeException(NativeContext);

            var global = VM.JS_GetGlobalObject(NativeContext);
            try
            {
                if (VM.JS_SetPropertyStr(NativeContext, global, nameMarshaller.ToUnmanaged(), callback) < 0)
                    throw TakeException(NativeContext);
            }
            finally
            {
                VM.JS_FreeValue(NativeContext, global);
            }
        }
        finally
        {
            nameMarshaller.Free();

            Runtime.Leave();
        }
    }

    private static VM.JSValue Invoke(VM.JSContext* pointer, VM.JSValue self, int count, VM.JSValue* arguments, int magic)
    {
        try
        {
            var weak = (WeakReference<QuickJsContext>)GCHandle.FromIntPtr((nint)VM.JS_GetContextOpaque(pointer)).Target!;
            if (!weak.TryGetTarget(out var context))
                throw new ObjectDisposedException(nameof(QuickJsContext));
            if (count > MaximumCallbackArguments)
                throw new ArgumentOutOfRangeException(nameof(count), "Callback argument limit exceeded.");

            var values = new double[count];
            for (var i = 0; i < count; i++)
            {
                double number = 0;
                if (VM.JS_ToFloat64(pointer, &number, arguments[i]) < 0)
                    return new VM.JSValue { tag = VM.JS_TAG_EXCEPTION };

                values[i] = number;
            }

            var result = context._callbacks[magic](values);
            return new VM.JSValue { tag = VM.JS_TAG_FLOAT64, u = new VM.JSValueUnion { float64 = result } };
        }
        catch (Exception error)
        {
            return ThrowManaged(pointer, error);
        }
    }

    internal static VM.JSValue ThrowManaged(VM.JSContext* context, Exception error)
    {
        VM.JSValue exception = default;
        var ownsException = false;

        scoped var messageMarshaller = new QuickJsText.ManagedToUnmanagedIn();
        try
        {
            exception = VM.JS_NewError(context);
            if (exception.tag == VM.JS_TAG_EXCEPTION)
                return exception;

            ownsException = true;

            messageMarshaller.FromManaged(error.Message, stackalloc byte[QuickJsText.ManagedToUnmanagedIn.BufferSize]);
            fixed (byte* key = "message\0"u8)
            {
                var value = VM.JS_NewStringLen(context, messageMarshaller.ToUnmanaged(), (ulong)messageMarshaller.Count);
                if (value.tag == VM.JS_TAG_EXCEPTION)
                    return value;

                if (VM.JS_SetPropertyStr(context, exception, key, value) < 0)
                    return new VM.JSValue { tag = VM.JS_TAG_EXCEPTION };
            }

            ownsException = false;
            return VM.JS_Throw(context, exception);
        }
        catch
        {
            return VM.JS_ThrowOutOfMemory(context);
        }
        finally
        {
            if (ownsException)
                VM.JS_FreeValue(context, exception);
        }
    }

    internal static QuickJsException TakeException(VM.JSContext* context)
    {
        var error = VM.JS_GetException(context);
        try
        {
            return new QuickJsException(String(context, error));
        }
        finally
        {
            VM.JS_FreeValue(context, error);
        }
    }

    internal static string String(VM.JSContext* context, VM.JSValue value)
    {
        ulong length = 0;
        var text = VM.JS_ToCStringLen2(context, &length, value, Cesu8Encoding);
        if (text == null)
        {
            // User-defined toString can throw, including while formatting an
            // exception/rejection. Consume that pending exception once without
            // recursively attempting to format another throwing object.
            var conversionError = VM.JS_GetException(context);
            VM.JS_FreeValue(context, conversionError);
            throw new QuickJsException("QuickJS string conversion failed.");
        }

        try
        {
            return QuickJsText.Decode(text, length);
        }
        finally
        {
            VM.JS_FreeCString(context, text);
        }
    }

    internal void Forget(QuickJsValue value) => _values.Remove(value);

    public void Dispose()
    {
        if (NativeContext == null)
            return;

        Runtime.Enter();
        try
        {
            Release();
        }
        finally
        {
            Runtime.Leave();
        }
    }

    internal void Release()
    {
        if (NativeContext == null)
            return;

        while (_values.Count != 0)
        {
            using var iterator = _values.GetEnumerator();
            iterator.MoveNext();
            iterator.Current.Release();
        }

        _values.Clear();
        VM.JS_FreeContext(NativeContext);
        NativeContext = null;
        // Native functions and jobs can retain their realm after the facade
        // context is disposed. Callback roots belong to the runtime lifetime.
    }

    internal void ReleaseCallbackRoots()
    {
        _callbacks.Clear();
        if (_handle.IsAllocated)
            _handle.Free();
    }
}