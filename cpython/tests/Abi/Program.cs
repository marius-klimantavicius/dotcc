using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using VM = Managed.Interpreters.CPython;

// Direct access to the real emitted types and exports. No mirror C structs,
// generated-source changes, reflection adapter, or alternate interpreter.
internal static unsafe class Program
{
    private static int assertions;

    private struct ObjectAlignment
    {
        public byte Prefix;
        public VM._object Value;
    }

    private static int Size<T>(T value) where T : unmanaged => sizeof(T);
    private static int HashSize<T>(delegate*<VM._object*, T> slot) where T : unmanaged => sizeof(T);
    private static int WideSize<T>(T** items) where T : unmanaged => sizeof(T);
    private static int PointeeSize<T>(T* data) where T : unmanaged => sizeof(T);

    private static void Require(bool condition, string operation)
    {
        assertions++;
        if (!condition) throw new InvalidOperationException("Actual CPython ABI assertion: " + operation);
    }

    private static Dictionary<string, int> Layout(string receipt)
    {
        using var provenance = JsonDocument.Parse(File.ReadAllText(receipt));
        int configWchar = int.Parse(provenance.RootElement.GetProperty("effective_pyconfig")
            .GetProperty("values").GetProperty("SIZEOF_WCHAR_T").GetString()!,
            System.Globalization.CultureInfo.InvariantCulture);
        VM._object obj = default;
        VM.PyVarObject variable = default;
        VM._typeobject type = default;
        VM.PyStatus status = default;
        VM.PyConfig config = default;
        VM.PyWideStringList wide = default;
        VM.PyUnicodeObject unicode = default;
        // This assignment also makes a signed/32-bit target wchar signature a
        // compile error, rather than reporting a hard-coded unsigned claim.
        char** wideItems = wide.items;
        ObjectAlignment aligned = default;
        // ob_refcnt is an actual anonymous-union forwarding property. Locate
        // its storage by a unique byte pattern written through that property;
        // do not depend on a generated anonymous-type hash or invent an offset.
        const long marker = 0x0706050403020100;
        obj.ob_refcnt = marker;
        int references = -1;
        for (int i = 0; i <= sizeof(VM._object) - sizeof(long); i++)
            if (Unsafe.ReadUnaligned<long>((byte*)&obj + i) == marker)
            {
                Require(references == -1, "unique refcount storage");
                references = i;
            }
        Require(references >= 0, "refcount property addresses actual object bytes");
        return new()
        {
            ["pointer"] = sizeof(void*), ["long"] = sizeof(long),
            ["wchar_t"] = WideSize(wideItems), ["config_wchar_t"] = configWchar,
            ["wchar_unsigned"] = (char)0xffff > 0 ? 1 : 0,
            ["Py_ssize_t"] = Size(variable.ob_size), ["Py_hash_t"] = HashSize(type.tp_hash),
            ["Py_UCS1"] = PointeeSize(unicode.data.latin1),
            ["Py_UCS2"] = PointeeSize(unicode.data.ucs2), ["Py_UCS4"] = PointeeSize(unicode.data.ucs4),
            ["PyObject"] = sizeof(VM._object),
            ["PyObject_align"] = (int)((byte*)&aligned.Value - (byte*)&aligned),
            ["ob_refcnt"] = references, ["ob_type"] = (int)((byte*)&obj.ob_type - (byte*)&obj),
            ["PyVarObject"] = sizeof(VM.PyVarObject),
            ["ob_size"] = (int)((byte*)&variable.ob_size - (byte*)&variable),
            ["PyTypeObject"] = sizeof(VM._typeobject),
            ["tp_repr"] = (int)((byte*)&type.tp_repr - (byte*)&type),
            ["tp_call"] = (int)((byte*)&type.tp_call - (byte*)&type),
            ["PyStatus"] = sizeof(VM.PyStatus),
            ["status_func"] = (int)((byte*)&status.func - (byte*)&status),
            ["status_err_msg"] = (int)((byte*)&status.err_msg - (byte*)&status),
            ["status_exitcode"] = (int)((byte*)&status.exitcode - (byte*)&status),
            ["PyWideStringList"] = sizeof(VM.PyWideStringList),
            ["PyPreConfig"] = sizeof(VM.PyPreConfig), ["PyConfig"] = sizeof(VM.PyConfig),
            ["config_search_paths"] = (int)((byte*)&config.module_search_paths - (byte*)&config)
        };
    }

    private static VM._object* Double(VM._object* context, VM._object* argument)
    {
        // Static managed thunk with the emitted slot's actual calling shape.
        // Never allow a managed exception to escape into translated C.
        try
        {
            long value = VM.PyLong_AsLong(argument);
            if (value == -1 && VM.PyErr_Occurred() != null) return null;
            return VM.PyLong_FromLong(value * 2);
        }
        catch (Exception)
        {
            fixed (byte* message = "managed ABI callback failed\0"u8)
                VM.PyErr_SetString((VM._object*)VM.Globals.PyExc_RuntimeError, message);
            return null;
        }
    }

    private static void Operations()
    {
        Require(VM.Py_IsInitialized() != 0, "initialized interpreter");
        VM._object* number = VM.PyLong_FromLong(1000000);
        Require(number != null, "new non-immortal integer");
        try
        {
            long count = number->ob_refcnt;
            VM.Py_INCREF(number);
            Require(number->ob_refcnt == count + 1, "INCREF changes actual header");
            VM.Py_DECREF(number);
            Require(number->ob_refcnt == count, "DECREF restores actual header");
            fixed (VM._typeobject* integerType = &VM.Globals.PyLong_Type)
            fixed (VM._typeobject* metatype = &VM.Globals.PyType_Type)
            {
                Require(number->ob_type == integerType, "real type identity");
                Require(((VM._object*)integerType)->ob_type == metatype, "real metatype identity");
            }
            VM._object* repr = number->ob_type->tp_repr(number);
            Require(repr != null, "invoke actual tp_repr slot");
            try
            {
                fixed (byte* expected = "1000000\0"u8)
                    Require(VM.PyUnicode_CompareWithASCIIString(repr, expected) == 0, "slot result");
            }
            finally { if (repr != null) VM.Py_DECREF(repr); }

            fixed (VM._object* singleton = &VM.Globals._Py_NoneStruct)
            {
                VM._object* none = VM.Py_NewRef(singleton);
                long immortal = none->ob_refcnt;
                VM.Py_INCREF(none); VM.Py_DECREF(none);
                Require(none == singleton && none->ob_refcnt == immortal, "immortal identity and refcount");
                VM.Py_DECREF(none);
            }

            fixed (byte* build = "(idLs#)\0"u8)
            fixed (byte* parse = "idLs#\0"u8)
            fixed (byte* text = "a\0b\0"u8)
            {
                // Actual exported varargs lowering, signed sizes, double and
                // pointers. _SizeT is the PY_SSIZE_T_CLEAN API selected in C.
                VM._object* tuple = VM._Py_BuildValue_SizeT(build, 7, 1.25, 1234567890123L, text, 3L);
                Require(tuple != null, "real Py_BuildValue varargs");
                try
                {
                    int integer = 0; double real = 0; long large = 0, length = 0; byte* bytes = null;
                    Require(VM._PyArg_ParseTuple_SizeT(tuple, parse, &integer, &real, &large, &bytes, &length) != 0,
                        "real PyArg_ParseTuple varargs");
                    Require(integer == 7 && real == 1.25 && large == 1234567890123L, "varargs default numeric promotions");
                    Require(length == 3 && bytes[0] == 'a' && bytes[1] == 0 && bytes[2] == 'b', "varargs embedded NUL and ssize length");
                }
                finally { if (tuple != null) VM.Py_DECREF(tuple); }
            }

            fixed (byte* name = "double\0"u8)
            {
                VM.PyMethodDef method = new() { ml_name = name, ml_meth = &Double, ml_flags = 8 };
                VM._object* function = VM.PyCFunction_NewEx(&method, null, null);
                VM._object* argument = VM.PyLong_FromLong(21);
                Require(function != null && argument != null, "real METH_O callback objects");
                try
                {
                    VM._object** args = stackalloc VM._object*[1]; args[0] = argument;
                    VM._object* result = VM.PyObject_Vectorcall(function, args, 1, null);
                    Require(result != null, "real callback via vectorcall");
                    try { Require(VM.PyLong_AsLong(result) == 42 && VM.PyErr_Occurred() == null, "callback result and exception state"); }
                    finally { if (result != null) VM.Py_DECREF(result); }
                }
                finally
                {
                    if (argument != null) VM.Py_DECREF(argument);
                    if (function != null) VM.Py_DECREF(function);
                }
            }
        }
        finally { if (number != null) VM.Py_DECREF(number); }
    }

    private static string? Text(byte* value) => value == null ? null : MarshalText(value);
    private static string MarshalText(byte* value)
    {
        int length = 0; while (value[length] != 0) length++;
        return Encoding.UTF8.GetString(new ReadOnlySpan<byte>(value, length));
    }

    public static int Main(string[] args)
    {
        if (args.Length != 3 || args[0] is not ("--layout" or "--operations"))
            throw new ArgumentException("Expected --layout REPORT TRANSLATION_RECEIPT or --operations STDLIB REPORT.");
        string report = args[0] == "--layout" ? args[1] : args[2];
        try
        {
            if (args[0] == "--layout")
            {
                File.WriteAllText(report, JsonSerializer.Serialize(Layout(args[2]), AbiJsonContext.Default.DictionaryStringInt32) + "\n");
                return 0;
            }
            for (int cycle = 0; cycle < 2; cycle++)
            {
                void* context = VM.dcp_create();
                Require(context != null, "real Host context allocation");
                bool initialized = false;
                bool restored = false;
                try
                {
                    fixed (byte* library = Encoding.UTF8.GetBytes(Path.GetFullPath(args[1]) + "\0"))
                    fixed (byte* application = "\0"u8)
                        VM.dcp_initialize(context, library, application);
                    if (VM.dcp_status_kind(context) != 0)
                        throw new InvalidOperationException("Actual PyStatus: " + Text(VM.dcp_status_function(context)) + ": " + Text(VM.dcp_status_message(context)));
                    initialized = true;
                    VM.dcp_restore(context);
                    restored = true;
                    Operations();
                    int finalized = VM.dcp_finalize(context);
                    initialized = false;
                    Require(finalized == 0, "real finalization");
                    Require(VM.Py_IsInitialized() == 0, "interpreter stopped before next cycle");
                }
                finally
                {
                    if (initialized && restored) VM.dcp_finalize(context);
                    // Preserve uncertain partial-initialization state. Never
                    // manufacture a clean/reusable runtime after failed init.
                    if (VM.dcp_failure_clean(context) != 0) VM.dcp_destroy(context);
                }
            }
            File.WriteAllText(report, JsonSerializer.Serialize(new AbiResult("passed", 2, assertions,
                true, true, true, true, true, "translated"), AbiJsonContext.Default.AbiResult) + "\n");
            return 0;
        }
        catch (Exception error)
        {
            File.WriteAllText(report + ".failure.json", JsonSerializer.Serialize(new AbiFailure("failed", assertions,
                error.ToString(), "translated"), AbiJsonContext.Default.AbiFailure) + "\n");
            Console.Error.WriteLine(error);
            return 1;
        }
    }
}

internal sealed record AbiResult(string status, int cycles, int assertions, bool refcounts,
    bool immortals, bool slots, bool varargs, bool vectorcall, string candidate_kind);
internal sealed record AbiFailure(string status, int assertions, string error, string candidate_kind);

[JsonSerializable(typeof(Dictionary<string, int>))]
[JsonSerializable(typeof(AbiResult))]
[JsonSerializable(typeof(AbiFailure))]
internal partial class AbiJsonContext : JsonSerializerContext;
