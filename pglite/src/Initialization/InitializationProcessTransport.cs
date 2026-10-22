using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Managed.PGlite.Initialization;

public enum BootstrapOperation { Version, Check, Bootstrap, SingleUser }

/// <summary>An invocation of the original translated PostgreSQL entrypoint.
/// Arguments include argv[0]. The adapter must use a fresh translated program
/// context, bind its real stdin/stdout files, run main, and return its exit code.
/// No shell, native PostgreSQL, or replacement bootstrap implementation belongs
/// behind this contract.</summary>
public sealed record BootstrapInvocation(BootstrapOperation Operation,
    IReadOnlyList<string> Arguments, string InputPath, string OutputPath,
    bool MergeStandardError)
{
    public string ErrorPath => OutputPath + ".stderr";
}

/// <summary>Opaque handles belong to initdb's emitted Libc, not the backend's.
/// Implement these calls using that runtime's fopen/fflush/fclose and errno.
/// Every call, including callbacks from pglitec, must bind the frontend owner.</summary>
public interface IInitdbFiles
{
    nint Open(string path, string mode);
    int Flush(nint stream);
    int Close(nint stream);
    void SetErrno(int error);
}

/// <summary>Implements the fork's system/popen/pclose callbacks in-process.
/// A write pipe spools initdb's unchanged BKI/SQL to a private file and runs the
/// real backend when pclose supplies EOF. A read pipe runs first, then opens
/// the real output. This changes scheduling, not any database algorithm.</summary>
public sealed class InitializationProcessTransport : IDisposable
{
    private const int EIO = 5, EBADF = 9, EBUSY = 16, EINVAL = 22, ENOTSUP = 95;
    private readonly IInitdbFiles files;
    private readonly Func<BootstrapInvocation, int> invoke;
    private readonly string executable, directory;
    private readonly int thread = Environment.CurrentManagedThreadId;
    private readonly Dictionary<nint, Pending> streams = new();
    private bool disposed, executing;
    private int sequence;
    private sealed record Pending(BootstrapInvocation Invocation, bool Write, int? Status);

    public InitializationProcessTransport(string backendExecutable, string privateDirectory,
        IInitdbFiles files, Func<BootstrapInvocation, int> invoke)
    {
        ArgumentException.ThrowIfNullOrEmpty(backendExecutable);
        ArgumentException.ThrowIfNullOrEmpty(privateDirectory);
        ArgumentNullException.ThrowIfNull(files);
        ArgumentNullException.ThrowIfNull(invoke);
        executable = backendExecutable;
        this.files = files;
        this.invoke = invoke;
        // The caller supplies a private parent. Own a fresh child, so cleanup
        // never removes preexisting data or a database directory.
        directory = Path.Combine(Path.GetFullPath(privateDirectory), "initdb-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
    }

    public Exception? LastError { get; private set; }

    private void RequireEntry()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (thread != Environment.CurrentManagedThreadId)
            throw new InvalidOperationException("Initdb callbacks must stay on their owning thread.");
        if (executing) throw new InvalidOperationException("Initdb callback reentry is unsupported.");
    }

    private int Execute(BootstrapInvocation request)
    {
        executing = true;
        try
        {
            int code = invoke(request);
            if ((uint)code > 255) throw new InvalidOperationException("Backend adapter must return an exit code, not a wait status or PGlite yield.");
            if (code is 99 or 100) throw new InvalidOperationException("Bootstrap unexpectedly yielded as an active PGlite SQL session.");
            return code << 8; // POSIX WIFEXITED/WEXITSTATUS, as checked by pclose_check.
        }
        finally { executing = false; }
    }

    private BootstrapInvocation Request(string command)
    {
        var parsed = InitdbCommand.Parse(command, executable);
        string name = Path.Combine(directory, (++sequence).ToString(global::System.Globalization.CultureInfo.InvariantCulture));
        string input = name + ".stdin", output = name + ".stdout";
        File.WriteAllBytes(input, []);
        File.WriteAllBytes(output, []);
        return new(parsed.Operation, parsed.Arguments, input, output, parsed.MergeStandardError);
    }

    private int Fail(Exception error)
    {
        LastError = error;
        files.SetErrno(error is NotSupportedException ? ENOTSUP : error is ArgumentException ? EINVAL :
            error is InvalidOperationException ? EBUSY : EIO);
        return -1;
    }

    /// <summary>Callback boundary: exceptions become errno and -1.</summary>
    public int System(string command)
    {
        try
        {
            RequireEntry();
            var request = Request(command);
            if (request.Operation != BootstrapOperation.Check)
                throw new NotSupportedException("Only initdb's --check uses system().");
            return Execute(request);
        }
        catch (Exception error) { return Fail(error); }
    }

    /// <summary>Callback boundary: exceptions become errno and NULL.</summary>
    public nint Open(string command, string mode)
    {
        try
        {
            RequireEntry();
            if (mode is not ("r" or "w")) throw new ArgumentException("Initdb pipes require r or w.");
            if (streams.Count != 0) throw new InvalidOperationException("Initdb opens one backend pipe at a time.");
            var request = Request(command);
            bool write = mode == "w";
            if (write ? request.Operation is not (BootstrapOperation.Bootstrap or BootstrapOperation.SingleUser) :
                request.Operation != BootstrapOperation.Version)
                throw new NotSupportedException("Command and pipe direction do not match initdb's protocol.");
            int? status = write ? null : Execute(request);
            nint handle = files.Open(write ? request.InputPath : request.OutputPath, mode);
            if (handle == 0) return 0; // Preserve fopen's actual errno.
            streams.Add(handle, new(request, write, status));
            return handle;
        }
        catch (Exception error) { Fail(error); return 0; }
    }

    public int Close(nint stream)
    {
        try
        {
            RequireEntry();
            if (!streams.TryGetValue(stream, out var pending)) { files.SetErrno(EBADF); return -1; }
            // fclose flushes and supplies EOF. Do not execute a partial BKI/SQL
            // stream if the frontend's real flush/close reports an error.
            bool failed = false;
            Exception? flushError = null;
            try { failed = pending.Write && files.Flush(stream) != 0; }
            catch (Exception error) { flushError = error; }
            int close = files.Close(stream);
            streams.Remove(stream);
            if (flushError != null) return Fail(flushError);
            if (failed || close != 0) return -1;
            return pending.Status ?? Execute(pending.Invocation);
        }
        catch (Exception error) { return Fail(error); }
    }

    public void Dispose()
    {
        if (disposed) return;
        RequireEntry();
        // Abandoned streams are closed, never executed during cleanup.
        Exception? failure = null;
        foreach (nint handle in streams.Keys)
            try { if (files.Close(handle) != 0) failure ??= new IOException("Closing an abandoned initdb stream failed."); }
            catch (Exception error) { failure ??= error; }
        streams.Clear();
        disposed = true;
        try { Directory.Delete(directory, true); }
        catch (Exception error) { failure ??= error; }
        if (failure != null) throw new IOException("Initdb transport cleanup failed.", failure);
    }
}

/// <summary>Parser for the exact shell command grammar generated by initdb and
/// common/exec.c. It decodes POSIX quoting but never evaluates shell syntax.</summary>
public sealed record InitdbCommand(BootstrapOperation Operation,
    IReadOnlyList<string> Arguments, bool MergeStandardError)
{
    private readonly record struct Token(string Text, bool Operator);

    public static InitdbCommand Parse(string command, string backendExecutable)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.IndexOf('\0') >= 0) throw new ArgumentException("NUL in initdb command.");
        var tokens = Tokenize(command);
        var args = new List<string>();
        bool redirected = false, input = false, output = false, merge = false;
        for (int i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];
            if (!token.Operator)
            {
                if (redirected) throw new NotSupportedException("Arguments after redirection are outside initdb's grammar.");
                args.Add(token.Text);
                continue;
            }
            redirected = true;
            if (token.Text == "2>&1")
            {
                if (!output || merge) throw new NotSupportedException("Unexpected stderr redirection.");
                merge = true;
                continue;
            }
            if (++i == tokens.Count || tokens[i].Operator || tokens[i].Text != "/dev/null")
                throw new NotSupportedException("Only initdb's /dev/null redirections are accepted.");
            if (token.Text == "<" ? input : output) throw new NotSupportedException("Repeated redirection.");
            if (token.Text == "<") input = true; else output = true;
        }
        if (args.Count < 2 || !string.Equals(args[0], backendExecutable, StringComparison.Ordinal))
            throw new NotSupportedException("Initdb may invoke only its exact configured translated backend.");
        BootstrapOperation operation = args[1] switch
        {
            "-V" when args.Count == 2 && !redirected => BootstrapOperation.Version,
            "--check" when input && output && merge => BootstrapOperation.Check,
            "--boot" when !redirected => BootstrapOperation.Bootstrap,
            "--single" when !input && output && !merge => BootstrapOperation.SingleUser,
            _ => throw new NotSupportedException("Unsupported initdb backend invocation.")
        };
        return new(operation, args.AsReadOnly(), merge);
    }

    private static List<Token> Tokenize(string text)
    {
        var result = new List<Token>();
        for (int i = 0; i < text.Length;)
        {
            if (text[i] is ' ' or '\t') { i++; continue; }
            if (text[i] is '\r' or '\n') throw new NotSupportedException("Unquoted shell line separators are unsupported.");
            if (text.AsSpan(i).StartsWith("2>&1", StringComparison.Ordinal))
            { result.Add(new("2>&1", true)); i += 4; continue; }
            if (text[i] is '<' or '>') { result.Add(new(text[i++].ToString(), true)); continue; }
            var word = new StringBuilder();
            bool started = false;
            while (i < text.Length && text[i] is not (' ' or '\t' or '<' or '>'))
            {
                char ch = text[i++];
                started = true;
                if (ch == '\'')
                {
                    int end = text.IndexOf('\'', i);
                    if (end < 0) throw new ArgumentException("Unclosed single quote.");
                    word.Append(text, i, end - i); i = end + 1;
                }
                else if (ch == '"')
                {
                    bool closed = false;
                    while (i < text.Length)
                    {
                        ch = text[i++];
                        if (ch == '"') { closed = true; break; }
                        if (ch == '\\')
                        {
                            if (i == text.Length) throw new ArgumentException("Trailing escape.");
                            char next = text[i++];
                            if (next is not ('"' or '\\' or '$' or '`' or '\n')) word.Append('\\');
                            if (next != '\n') word.Append(next);
                        }
                        else
                        {
                            if (ch is '$' or '`') throw new NotSupportedException("Shell expansion is unsupported.");
                            word.Append(ch);
                        }
                    }
                    if (!closed) throw new ArgumentException("Unclosed double quote.");
                }
                else if (ch == '\\')
                {
                    if (i == text.Length) throw new ArgumentException("Trailing escape.");
                    ch = text[i++];
                    if (ch != '\n') word.Append(ch);
                }
                else
                {
                    if (ch is '$' or '`' or ';' or '|' or '&' or '(' or ')' or '*' or '?' or '[' or ']' or '~' or '#' or '\r' or '\n')
                        throw new NotSupportedException("Shell operators and expansions are unsupported.");
                    word.Append(ch);
                }
            }
            if (started) result.Add(new(word.ToString(), false));
        }
        return result;
    }
}
