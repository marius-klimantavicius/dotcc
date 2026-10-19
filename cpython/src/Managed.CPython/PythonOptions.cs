namespace Managed.CPython;

public sealed class PythonOptions
{
    public string StandardLibraryDirectory { get; }
    public string ApplicationDirectory { get; }

    /// <summary>Allow CPython to write import caches in consumer-owned directories.</summary>
    public bool WriteBytecode { get; init; }

    /// <summary>Select CPython UTF-8 mode during isolated preinitialization.</summary>
    public bool Utf8Mode { get; init; } = true;

    public PythonOptions(string standardLibraryDirectory, string applicationDirectory)
    {
        StandardLibraryDirectory = ValidateDirectory(standardLibraryDirectory, nameof(standardLibraryDirectory));
        ApplicationDirectory = ValidateDirectory(applicationDirectory, nameof(applicationDirectory));
    }

    private static string ValidateDirectory(string path, string name)
    {
        PythonText.RequireCString(path, name);
        string absolute = Path.GetFullPath(path);
        if (!Directory.Exists(absolute)) throw new DirectoryNotFoundException(absolute);
        return absolute;
    }
}

public enum PythonStatusKind { Ok, Error, Exit }

// The binding copies PyStatus.func/err_msg before clearing PyConfig. Exit is a
// returned status, never Py_ExitStatusException (which terminates the process).
public sealed record PythonInitializationStatus(PythonStatusKind Kind, string? Function, string? Message, int ExitCode)
{
    internal void ThrowIfNotOk()
    {
        if (Kind != PythonStatusKind.Ok) throw new PythonInitializationException(this);
    }
}

public sealed class PythonInitializationException(PythonInitializationStatus status)
    : Exception($"CPython initialization {status.Kind}: {status.Function}: {status.Message} (exit {status.ExitCode})")
{
    public PythonInitializationStatus Status { get; } = status;
}

public sealed class PythonException(string pythonType, string message, string traceback,
    string standardOutput = "", string standardError = "") : Exception(message)
{
    public string PythonType { get; } = pythonType;
    public string PythonTraceback { get; } = traceback;
    public string StandardOutput { get; } = standardOutput;
    public string StandardError { get; } = standardError;
}

public sealed record PythonExecutionResult(string StandardOutput, string StandardError);
