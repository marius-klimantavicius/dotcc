namespace Managed.CPython;

public sealed partial class PythonRuntime
{
    // The direct adapter requires the actual translated product, including the
    // ordinary translated Host C unit. There is no native or fake fallback.
    public static PythonRuntime Initialize(PythonOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return new PythonRuntime(TranslatedExports.Create(), options);
    }
}
