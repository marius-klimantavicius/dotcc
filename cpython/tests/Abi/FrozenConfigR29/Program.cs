using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Managed.CPython;
using VM = Managed.Interpreters.CPython;

// API semantic control on an unchanged actual interpreter PE. This does not
// call its old dcp_initialize or claim that the frozen owning Host is repaired.
internal static unsafe class Program
{
    private static int checks;
    private static void Check(bool value, string label)
    { checks++; if (!value) throw new Exception(label); }
    private static string? Text(byte* text) => text == null ? null : Marshal.PtrToStringUTF8((nint)text);
    private static void Ok(VM.PyStatus status)
    {
        if (status._type != 0) throw new Exception($"PyStatus {status._type}: {Text(status.func)} {Text(status.err_msg)}");
    }
    private static VM.PyStatus AddPath(VM.PyConfig* config, string path, bool standardLibrary)
    {
        char[] temporary = (path + "\0").ToCharArray();
        VM.PyStatus status;
        fixed (char* wide = temporary)
        {
            status = standardLibrary
                ? VM.PyConfig_SetString(config, &config->stdlib_dir, wide)
                : default;
            if (standardLibrary && status._type == 0)
                Check(config->stdlib_dir != wide && new string(config->stdlib_dir) == path, "PyConfig owns an independent UTF16 stdlib copy");
            if (status._type == 0) status = VM.PyWideStringList_Append(&config->module_search_paths, wide);
            if (status._type == 0)
                Check(config->module_search_paths.items[config->module_search_paths.length - 1] != wide, "search path owns a separate wide copy");
        }
        Array.Fill(temporary, 'x'); // copied paths must outlive these input bytes
        GC.Collect(2, GCCollectionMode.Forced, true, true);
        return status;
    }
    private static int Run(string script)
    {
        fixed (byte* source = Encoding.UTF8.GetBytes(script + "\0"))
            return VM.PyRun_SimpleString(source);
    }
    private static int Main(string[] args)
    {
        if (args[0] == "layout")
        {
            var layout = new Dictionary<string, long> {
                ["config_size"] = sizeof(VM.PyConfig), ["preconfig_size"] = sizeof(VM.PyPreConfig),
                ["wchar_size"] = sizeof(char), ["list_size"] = sizeof(VM.PyWideStringList),
                ["list_length"] = (long)Marshal.OffsetOf<VM.PyWideStringList>("length"),
                ["list_items"] = (long)Marshal.OffsetOf<VM.PyWideStringList>("items") };
            foreach (string field in new[] { "program_name", "stdlib_dir", "module_search_paths", "module_search_paths_set",
                         "install_signal_handlers", "site_import", "parse_argv", "write_bytecode" })
                layout[field] = (long)Marshal.OffsetOf<VM.PyConfig>(field);
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(layout));
            return 0;
        }
        bool includeStdlib = args[0] != "before";
        bool expectError = args[0] == "error";
        // Exercise the public facade's existing invalid-directory contract too.
        bool rejected = false;
        try { _ = new PythonOptions(args[1] + "/absent-b29-directory", args[2]); }
        catch (DirectoryNotFoundException) { rejected = true; }
        Check(rejected && VM.Py_IsInitialized() == 0, "invalid options reject before startup");
        var options = new PythonOptions(args[1], args[2]);
        for (int cycle = 0; cycle < 2; cycle++)
        {
            VM.PyPreConfig pre = default;
            VM.PyPreConfig_InitIsolatedConfig(&pre); pre.utf8_mode = 1;
            Ok(VM.Py_PreInitialize(&pre));
            VM.PyConfig config = default;
            VM.PyConfig_InitIsolatedConfig(&config);
            config.install_signal_handlers = 0; config.site_import = 0;
            config.parse_argv = 0; config.write_bytecode = 0;
            config.module_search_paths_set = 1;
            VM.PyStatus status;
            string? function = null, message = null;
            try
            {
                fixed (char* name = "dotcc-cpython\0") Ok(VM.PyConfig_SetString(&config, &config.program_name, name));
                Ok(AddPath(&config, options.StandardLibraryDirectory, includeStdlib));
                Ok(AddPath(&config, options.ApplicationDirectory, false));
                if (includeStdlib) Check(new string(config.stdlib_dir) == options.StandardLibraryDirectory, "owned stdlib survives input overwrite and GC");
                Check(new string(config.module_search_paths.items[0]) == options.StandardLibraryDirectory, "selected search path survives input overwrite and GC");
                status = VM.Py_InitializeFromConfig(&config);
                function = Text(status.func); message = Text(status.err_msg);
            }
            finally
            {
                VM.PyConfig_Clear(&config);
                Check(config.stdlib_dir == null && config.module_search_paths.length == 0 && config.module_search_paths.items == null,
                    "PyConfig_Clear releases and clears owned wide inputs");
            }
            if (status._type != 0)
            {
                Console.WriteLine($"actual API PyStatus kind={status._type} function={function} message={message} initialized={VM.Py_IsInitialized()}");
                return expectError && status._type == 1 && message != null ? 23 : 26;
            }
            if (expectError) return 27;
            int result;
            try { result = Run(File.ReadAllText(args[3])); }
            finally { Check(VM.Py_FinalizeEx() == 0 && VM.Py_IsInitialized() == 0, "actual finalization complete"); }
            Console.WriteLine($"actual typed PyConfig cycle={cycle} result={result} dynamicCode={RuntimeFeature.IsDynamicCodeSupported}");
            if (result != 0) return 24;
        }
        Console.WriteLine($"actual typed PyConfig lifecycles=2 assertions={checks}; unchanged frozen Host/runtime are separate");
        return 0;
    }
}
