namespace Managed.Interpreters;

/// <summary>An owning JS value. Context/runtime disposal invalidates outstanding values.</summary>
public sealed unsafe class QuickJsValue : IDisposable
{
    private bool _disposed;

    internal readonly QuickJsContext Context;
    internal QuickJs.JSValue Raw;

    internal QuickJsValue(QuickJsContext context, QuickJs.JSValue value)
    {
        Context = context;
        Raw = value;
    }

    internal void Check(QuickJsContext context)
    {
        Context.Check();
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (!ReferenceEquals(Context.Runtime, context.Runtime))
            throw new ArgumentException("Values cannot cross QuickJS runtime boundaries.");
    }

    public QuickJsValue Duplicate()
    {
        Context.Runtime.Enter();
        try
        {
            Check(Context);
            return Context.Own(QuickJs.JS_DupValue(Context.NativeContext, Raw));
        }
        finally
        {
            Context.Runtime.Leave();
        }
    }

    public QuickJsValue GetProperty(string name)
    {
        QuickJsText.CheckName(name);
        Context.Runtime.Enter();

        scoped var nameMarshaller = new QuickJsText.ManagedToUnmanagedIn();
        try
        {
            Check(Context);
            nameMarshaller.FromManaged(name, stackalloc byte[QuickJsText.ManagedToUnmanagedIn.BufferSize]);
            return Context.Own(QuickJs.JS_GetPropertyStr(Context.NativeContext, Raw, nameMarshaller.ToUnmanaged()));
        }
        finally
        {
            nameMarshaller.Free();
            
            Context.Runtime.Leave();
        }
    }

    public override string ToString()
    {
        Context.Runtime.Enter();
        try
        {
            Check(Context);
            return QuickJsContext.String(Context.NativeContext, Raw);
        }
        finally { Context.Runtime.Leave(); }
    }

    public double ToDouble()
    {
        Context.Runtime.Enter();
        try
        {
            Check(Context);
            double result = 0;
            if (QuickJs.JS_ToFloat64(Context.NativeContext, &result, Raw) < 0)
                throw QuickJsContext.TakeException(Context.NativeContext);

            return result;
        }
        finally
        {
            Context.Runtime.Leave();
        }
    }

    /// <summary>Copies an ArrayBuffer into managed storage; no engine pointer escapes.</summary>
    public byte[] ToArrayBuffer()
    {
        Context.Runtime.Enter();
        try
        {
            Check(Context);
            ulong length = 0;
            var pointer = QuickJs.JS_GetArrayBuffer(Context.NativeContext, &length, Raw);

            if (pointer == null && QuickJs.JS_HasException(Context.NativeContext) != 0)
                throw QuickJsContext.TakeException(Context.NativeContext);
            if (length > int.MaxValue)
                throw new OverflowException("ArrayBuffer exceeds managed array capacity.");

            return new ReadOnlySpan<byte>(pointer, (int)length).ToArray();
        }
        finally
        {
            Context.Runtime.Leave();
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        Context.Runtime.Enter();
        try
        {
            Release();
        }
        finally
        {
            Context.Runtime.Leave();
        }
    }

    internal void Release()
    {
        if (_disposed)
            return;

        _disposed = true;
        QuickJs.JS_FreeValue(Context.NativeContext, Raw);
        Context.Forget(this);
    }
}