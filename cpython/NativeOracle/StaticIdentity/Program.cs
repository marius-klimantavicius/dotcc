using VM = Managed.Interpreters.CPython;

internal static unsafe class Program
{
    private static void Require(bool value)
    {
        if (!value) throw new InvalidOperationException("Actual CPython static identity");
    }

    public static void Main()
    {
        // Pin the actual emitted globals for every pointer use, including the
        // compacting collection. No borrowed address is retained after the pins.
        fixed (VM._typeobject* type = &VM.Globals.PyType_Type)
        fixed (VM._typeobject* integer = &VM.Globals.PyLong_Type)
        fixed (VM._typeobject* boolean = &VM.Globals.PyBool_Type)
        fixed (VM._object* none = &VM.Globals._Py_NoneStruct)
        fixed (VM._longobject* yes = &VM.Globals._Py_TrueStruct)
        fixed (VM._longobject* no = &VM.Globals._Py_FalseStruct)
        {
            for (int cycle = 0; cycle < 2; cycle++)
            {
                Require(((VM._object*)type)->ob_type == type);
                Require(((VM._object*)integer)->ob_type == type);
                Require(((VM._object*)boolean)->ob_type == type);
                Require(boolean->tp_base == integer);
                Require(((VM._object*)yes)->ob_type == boolean);
                Require(((VM._object*)no)->ob_type == boolean);
                Require(yes != no && (void*)none != yes && (void*)none != no);
                Require(none->ob_type != null && none->ob_type != integer && none->ob_type != boolean);
                Require(new ReadOnlySpan<byte>(integer->tp_name, 4).SequenceEqual("int\0"u8));
                Require(new ReadOnlySpan<byte>(boolean->tp_name, 5).SequenceEqual("bool\0"u8));
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            }
        }
        Console.WriteLine("static identity: 20 checks, exact names and singleton/type/boolean base graph");
    }
}
