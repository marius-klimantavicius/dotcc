using VM = Managed.Interpreters.CPython;

namespace Managed.CPython;

// Direct, statically reachable calls only. Signatures were inspected in the
// untouched limited Host object/link probe (see api-progress.md). No reflection,
// native library loading, generated-source repairs, or alternate backend.
internal sealed unsafe class TranslatedExports : CPythonExports
{
    private void* context;
    private bool initializationFailureIsClean = true;
    // This product is a STATIC C program. Its global allocator state/pins may
    // survive Py_Finalize; a RuntimeContext owns the whole program lifetime,
    // not one PythonRuntime generation. The process reservation serializes all
    // accesses to this binding. Reject a different owner before translated entry.
    private static VM.Libc.RuntimeContext? programOwner;
    private static bool programExplicitlyBound;
    private CapturedContext? capturedContext;
    internal static CPythonExports Create() => new TranslatedExports();
    internal override bool InitializationFailureIsClean => initializationFailureIsClean;

    internal override PythonExecutionContext CaptureExecutionContext()
    {
        var captured = new CapturedContext();
        try { captured.RequireProgramBinding(); capturedContext = captured; return captured; }
        catch { captured.Dispose(); throw; }
    }

    // This is the Libc compiled INTO the selected translated project. Adding a
    // DotCC.Libc project reference here would create a second runtime identity.
    private sealed class CapturedContext : PythonExecutionContext
    {
        private readonly VM.Libc.RuntimeContext owner = VM.Libc.CaptureRuntimeContext();
        private readonly bool explicitlyBound = VM.Libc.RuntimeContext.Current != null;
        private readonly IDisposable retained;
        private IDisposable? policy;
        internal CapturedContext() => retained = owner.RetainLease();
        internal void RequireProgramBinding()
        {
            if (programOwner != null && (!ReferenceEquals(programOwner, owner) || programExplicitlyBound != explicitlyBound))
                throw new InvalidOperationException("The static translated CPython program must retain its original Libc runtime context and nullable binding across Python runtime generations.");
        }
        internal void CommitProgramBinding()
        {
            RequireProgramBinding();
            programOwner = owner;
            programExplicitlyBound = explicitlyBound;
        }
        internal override IDisposable Enter()
        {
            // Null is the legacy binding, even though its runtime/policy owner
            // is an actual retained object. Turning it into an explicit binding
            // changes heap/path/termination contracts in the shared runtime.
            VM.Libc.RuntimeBinding binding = explicitlyBound ? owner.Enter() : default;
            try
            {
                policy = owner.UseMinimumPthreadStackSize(PythonExecutionThread.StackReservationBytes);
                return binding;
            }
            catch { binding.Dispose(); throw; }
        }
        public override void Dispose()
        {
            try { policy?.Dispose(); }
            finally { retained.Dispose(); }
        }
    }

    private static byte[] CString(string text) => PythonText.EncodeUtf8(PythonText.RequireCString(text, nameof(text)) + "\0");
    private static string? Text(byte* text)
    {
        if (text == null) return null;
        int length = 0;
        while (text[length] != 0) length = checked(length + 1);
        return PythonText.DecodeUtf8(new ReadOnlySpan<byte>(text, length));
    }

    internal override PythonInitializationStatus Initialize(PythonOptions options)
    {
        capturedContext!.CommitProgramBinding();
        context = VM.dcp_create();
        if (context == null) throw new OutOfMemoryException("CPython embedding context allocation failed.");
        initializationFailureIsClean = false;
        fixed (byte* library = CString(options.StandardLibraryDirectory))
        fixed (byte* application = CString(options.ApplicationDirectory))
            VM.dcp_initialize_configured(context, library, application,
                options.WriteBytecode ? 1 : 0, options.Utf8Mode ? 1 : 0);
        int kind = VM.dcp_status_kind(context);
        if (kind is < 0 or > 2) throw new InvalidOperationException("Invalid translated PyStatus kind.");
        var status = new PythonInitializationStatus((PythonStatusKind)kind,
            Text(VM.dcp_status_function(context)), Text(VM.dcp_status_message(context)), VM.dcp_status_exitcode(context));
        if (kind != 0)
        {
            initializationFailureIsClean = VM.dcp_failure_clean(context) != 0;
            string? detail = ErrorText(1);
            if (detail != null) status = status with { Message = status.Message + ": " + detail };
            if (initializationFailureIsClean) { VM.dcp_destroy(context); context = null; }
        }
        return status;
    }
    internal override void RestoreOwnerThread() => VM.dcp_restore(context);
    internal override void SaveOwnerThread() => VM.dcp_save(context);

    private readonly struct Operation : IDisposable
    {
        private readonly TranslatedExports exports;
        private readonly CancellationTokenRegistration registration;
        internal Operation(TranslatedExports exports, CancellationToken cancellationToken)
        {
            this.exports = exports;
            VM.dcp_operation_begin(exports.context, cancellationToken.CanBeCanceled ? 1 : 0);
            try
            {
                registration = cancellationToken.UnsafeRegister(static state =>
                    VM.dcp_request_cancel(((TranslatedExports)state!).context), exports);
            }
            catch { VM.dcp_operation_end(exports.context); throw; }
        }
        public void Dispose()
        {
            // Drain before resetting the atomic flag or freeing the context.
            registration.Dispose();
            VM.dcp_operation_end(exports.context);
        }
    }

    internal override PythonExecutionResult Execute(string source, string filename, CancellationToken cancellationToken)
    {
        using var operation = new Operation(this, cancellationToken);
        fixed (byte* code = CString(source))
        fixed (byte* file = CString(filename))
            VM.dcp_execute(context, code, file);
        return new PythonExecutionResult(Output(0), Output(1));
    }
    private string Output(int stream)
    {
        ulong length = 0;
        byte* data = VM.dcp_output(context, stream, &length);
        return length == 0 ? "" : PythonText.DecodeUtf8(new ReadOnlySpan<byte>(data, checked((int)length)));
    }
    internal override nint Evaluate(string expression, string filename, CancellationToken cancellationToken)
    {
        using var operation = new Operation(this, cancellationToken);
        fixed (byte* code = CString(expression))
        fixed (byte* file = CString(filename))
            return (nint)VM.dcp_evaluate(context, code, file);
    }
    internal override nint Import(string module)
    {
        fixed (byte* name = CString(module))
        {
            nint value = (nint)VM.PyImport_ImportModule(name);
            VM.dcp_capture_output(context);
            return value;
        }
    }
    internal override nint GetAttribute(nint value, string name)
    {
        fixed (byte* text = CString(name))
        {
            nint result = (nint)VM.PyObject_GetAttrString((VM._object*)value, text);
            VM.dcp_capture_output(context);
            return result;
        }
    }
    internal override nint Call(nint callable, ReadOnlySpan<nint> arguments, CancellationToken cancellationToken)
    {
        using var operation = new Operation(this, cancellationToken);
        if (VM.dcp_operation_poll(context) < 0) return 0;
        fixed (nint* values = arguments)
        {
            nint result = (nint)VM.PyObject_Vectorcall((VM._object*)callable, (VM._object**)values, (ulong)arguments.Length, null);
            VM.dcp_capture_output(context);
            return result;
        }
    }
    internal override nint FromInt64(long value) => (nint)VM.PyLong_FromLongLong(value);
    internal override long AsInt64(nint value) => VM.PyLong_AsLongLong((VM._object*)value);
    internal override nint FromCodePoints(ReadOnlySpan<uint> value)
    {
        fixed (uint* points = value)
            return (nint)VM.PyUnicode_FromKindAndData((int)VM.PyUnicode_4BYTE_KIND, points, value.Length);
    }
    internal override uint[] AsCodePoints(nint value)
    {
        long length = VM.PyUnicode_GetLength((VM._object*)value);
        if (length < 0) return []; // Pending real Python error is checked by owner.
        uint* points = VM.PyUnicode_AsUCS4Copy((VM._object*)value);
        if (points == null) return [];
        try { return new ReadOnlySpan<uint>(points, checked((int)length)).ToArray(); }
        finally { VM.PyMem_Free(points); }
    }
    internal override void IncRef(nint value) => VM.Py_INCREF((VM._object*)value);
    internal override void DecRef(nint value) => VM.Py_DECREF((VM._object*)value);
    internal override bool ErrorOccurred() => VM.PyErr_Occurred() != null;
    internal override PythonException TakeException()
    {
        VM.dcp_take_error(context);
        return new PythonException(ErrorText(0) ?? "<error capture failed>",
            ErrorText(1) ?? "<error capture failed>", ErrorText(2) ?? "<traceback capture failed>", Output(0), Output(1));
    }
    private string? ErrorText(int part)
    {
        byte* data = part switch {
            0 => VM.dcp_error_type(context), 1 => VM.dcp_error_message(context), _ => VM.dcp_error_traceback(context)
        };
        return data == null ? null : PythonText.DecodeUtf8(new ReadOnlySpan<byte>(data, checked((int)VM.dcp_error_length(context, part))));
    }
    internal override void RegisterInt64Callback(string module, string name, long token)
    {
        fixed (byte* owner = CString(module))
        fixed (byte* member = CString(name))
            VM.dcp_register_callback(context, owner, member, token, &CallbackThunks.Int64);
    }
    internal override void JoinPythonWorkers() => VM.dcp_join_workers(context);
    internal override int FinalizeRuntime()
    {
        int result = VM.dcp_finalize(context);
        VM.dcp_destroy(context); context = null;
        return result;
    }
}
