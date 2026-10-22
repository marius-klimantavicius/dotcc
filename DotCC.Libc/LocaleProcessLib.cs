#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    public sealed partial class RuntimeContext
    {
        internal int LocaleProcessCount
        {
            get
            {
                if (!localeProcessStates.TryGetValue(this, out var state)) return 0;
                lock (state.Gate) return state.Children.Count;
            }
        }
    }

    private sealed class LocaleProcessState
    {
        internal readonly object Gate = new();
        internal readonly Dictionary<nint, LocaleChild> Children = new();
    }
    private static readonly ConditionalWeakTable<RuntimeContext, LocaleProcessState> localeProcessStates = new();
    private static LocaleProcessState LocaleProcesses => localeProcessStates.GetValue(RuntimeState, static _ => new());

    private static bool LocaleInventoryCommand(byte* command)
    {
        ReadOnlySpan<byte> expected = "locale -a\0"u8;
        for (int i = 0; i < expected.Length; i++)
            if (command[i] != expected[i]) return false;
        return true;
    }

    private static FILE* OpenLocaleInventory(byte* mode)
    {
        // Use a fixed OS utility, never PATH, a shell, or caller-supplied argv.
        string executable = File.Exists("/usr/bin/locale") ? "/usr/bin/locale" : "/bin/locale";
        return OpenLocaleUtility(executable, mode);
    }

    // Internal only so host controls can exercise nonzero/signal/error cleanup
    // with real OS children. The exported popen route supplies only locale -a.
    internal static FILE* OpenLocaleUtility(string executable, byte* mode)
    {
        if (!OperatingSystem.IsLinux() || RuntimeInformation.ProcessArchitecture != Architecture.X64)
        { errno = ENOTSUP; return null; }
        LocaleChild? child = null;
        int descriptor = -1;
        FILE* stream = null;
        try
        {
            // The opaque action structure below is the glibc Linux LP64 ABI.
            // Do not pass its layout to another libc implementation.
            if (LocaleGlibcVersion() == null) { errno = ENOTSUP; return null; }
            child = StartLocaleChild(executable);
            if (child is null) return null;
            descriptor = RegisterFileSlot(child.Output);
            var slot = SlotByFd(descriptor)!;
            ConfigureStdioSlot(slot, mode, FileAccess.Read);
            stream = AllocateFileHandle(descriptor);
            child.Slot = slot;
            var processes = LocaleProcesses;
            lock (processes.Gate) processes.Children.Add((nint)stream, child);
            return stream;
        }
        catch (DllNotFoundException) { errno = ENOSYS; }
        catch (EntryPointNotFoundException) { errno = ENOSYS; }
        catch (UnauthorizedAccessException) { errno = EACCES; }
        catch (IOException) { errno = EIO; }
        catch (OutOfMemoryException) { errno = ENOMEM; }
        int error = errno;
        if (stream != null) fclose(stream);
        else if (descriptor >= 0) CloseSlot(SlotByFd(descriptor)!);
        child?.Abandon();
        errno = error;
        return null;
    }

    private static int CloseLocaleInventory(FILE* stream)
    {
        var processes = LocaleProcesses;
        LocaleChild? child;
        lock (processes.Gate)
        {
            if (!processes.Children.TryGetValue((nint)stream, out child) ||
                !ReferenceEquals(Slot(stream), child.Slot) ||
                !ReferenceEquals(child.Slot!.Stream, child.Output))
            { errno = ECHILD; return -1; }
            processes.Children.Remove((nint)stream);
        }
        int closed = fclose(stream);
        int closeError = errno;
        int status = child.Wait(terminate: false);
        int waitError = errno;
        try
        {
            byte[] errors = child.ReadErrors();
            fixed (byte* bytes = errors)
                if (errors.Length != 0 && fwrite(bytes, 1, errors.Length, stderr) != errors.Length)
                    return -1;
        }
        catch (IOException) { errno = EIO; return -1; }
        finally { child.DisposeErrorStream(); }
        if (closed != 0) { errno = closeError; return -1; }
        if (status < 0) errno = waitError;
        return status;
    }

    private static void DisposeLocaleProcesses(RuntimeContext owner)
    {
        if (!localeProcessStates.TryGetValue(owner, out var processes)) return;
        lock (processes.Gate)
        {
            // There is no ambient owner binding during disposal. Work only on
            // captured native child/stream state; file handles are freed next.
            foreach (var child in processes.Children.Values) child.Abandon();
            processes.Children.Clear();
        }
        localeProcessStates.Remove(owner);
    }

    private sealed class LocaleChild(int process, FileStream output, FileStream error)
    {
        internal readonly FileStream Output = output;
        internal FileSlot? Slot;
        private int pid = process;
        private readonly FileStream errorStream = error;
        // Drain independently of stdout so neither OS pipe can deadlock the
        // other. No runtime binding or borrowed FILE crosses to this worker.
        private readonly System.Threading.Tasks.Task<byte[]> errors = System.Threading.Tasks.Task.Run(() =>
        {
            using var bytes = new MemoryStream();
            error.CopyTo(bytes);
            return bytes.ToArray();
        });

        internal int Wait(bool terminate, bool reportError = true)
        {
            if (pid == 0) { if (reportError) errno = ECHILD; return -1; }
            int status;
            int result;
            do { result = LocaleWaitPid(pid, &status, terminate ? 1 : 0); }
            while (result < 0 && Marshal.GetLastPInvokeError() == EINTR);
            if (result == 0 && terminate)
            {
                LocaleKill(pid, 9);
                do { result = LocaleWaitPid(pid, &status, 0); }
                while (result < 0 && Marshal.GetLastPInvokeError() == EINTR);
            }
            int error = result < 0 ? Marshal.GetLastPInvokeError() : 0;
            if (result > 0 || error == ECHILD) pid = 0;
            if (result < 0) { if (reportError) errno = error; return -1; }
            return status;
        }

        internal byte[] ReadErrors() => errors.GetAwaiter().GetResult();
        internal void DisposeErrorStream() => errorStream.Dispose();
        internal void Abandon()
        {
            try { Output.Dispose(); } catch (IOException) { }
            Wait(terminate: true, reportError: false);
            try { ReadErrors(); } catch (IOException) { }
            DisposeErrorStream();
        }
    }

    // /usr/include/spawn.h, glibc Linux x64. No managed code runs in a forked
    // child: posix_spawn performs only native descriptor actions and exec.
    [StructLayout(LayoutKind.Sequential)]
    private struct LocaleSpawnActions
    {
        private int allocated, used;
        private void* actions;
        private fixed int padding[16];
    }

    private sealed class LocaleArguments : IDisposable
    {
        internal readonly byte** Values;
        private readonly nint[] strings;
        internal LocaleArguments(string[] values)
        {
            strings = new nint[values.Length];
            Values = (byte**)NativeMemory.AllocZeroed((nuint)(values.Length + 1), (nuint)sizeof(byte*));
            try
            {
                for (int i = 0; i < values.Length; i++)
                    Values[i] = (byte*)(strings[i] = Marshal.StringToCoTaskMemUTF8(values[i]));
            }
            catch { Dispose(); throw; }
        }
        public void Dispose()
        {
            foreach (nint value in strings) Marshal.FreeCoTaskMem(value);
            NativeMemory.Free(Values);
        }
    }

    private static LocaleChild? StartLocaleChild(string executable)
    {
        int* output = stackalloc int[2] { -1, -1 };
        int* errors = stackalloc int[2] { -1, -1 };
        LocaleSpawnActions actions = default;
        bool initialized = false;
        int pid = 0;
        FileStream? stdoutStream = null, stderrStream = null;
        try
        {
            if (LocalePipe(output, 0x80000) != 0 || LocalePipe(errors, 0x80000) != 0)
            { errno = Marshal.GetLastPInvokeError(); return null; }
            // Descriptor actions must not close a stdio destination that was
            // also returned by pipe2 on a host with closed standard handles.
            for (int i = 0; i < 2; i++)
                if (output[i] <= 2 || errors[i] <= 2) { errno = ENOTSUP; return null; }
            int result = LocaleActionsInit(&actions);
            if (result != 0) { errno = result; return null; }
            initialized = true;
            fixed (byte* devnull = "/dev/null\0"u8)
                result = LocaleActionsOpen(&actions, 0, devnull, 0, 0);
            if (result == 0) result = LocaleActionsDup(&actions, output[1], 1);
            if (result == 0) result = LocaleActionsDup(&actions, errors[1], 2);
            for (int i = 0; result == 0 && i < 2; i++) result = LocaleActionsClose(&actions, output[i]);
            for (int i = 0; result == 0 && i < 2; i++) result = LocaleActionsClose(&actions, errors[i]);
            if (result != 0) { errno = result; return null; }
            var environment = new List<string>();
            foreach (System.Collections.DictionaryEntry item in Environment.GetEnvironmentVariables())
                environment.Add((string)item.Key + "=" + item.Value);
            using var argv = new LocaleArguments([executable, "-a"]);
            using var envp = new LocaleArguments(environment.ToArray());
            result = LocaleSpawn(&pid, argv.Values[0], &actions, null, argv.Values, envp.Values);
            if (result != 0) { errno = result; pid = 0; return null; }
            LocaleClose(output[1]); output[1] = -1;
            LocaleClose(errors[1]); errors[1] = -1;
            var stdoutHandle = new Microsoft.Win32.SafeHandles.SafeFileHandle((nint)output[0], true);
            output[0] = -1;
            try { stdoutStream = new FileStream(stdoutHandle, FileAccess.Read); }
            catch { stdoutHandle.Dispose(); throw; }
            var stderrHandle = new Microsoft.Win32.SafeHandles.SafeFileHandle((nint)errors[0], true);
            errors[0] = -1;
            try { stderrStream = new FileStream(stderrHandle, FileAccess.Read); }
            catch { stderrHandle.Dispose(); throw; }
            var child = new LocaleChild(pid, stdoutStream, stderrStream);
            pid = 0;
            stdoutStream = stderrStream = null;
            return child;
        }
        finally
        {
            if (initialized) LocaleActionsDestroy(&actions);
            for (int i = 0; i < 2; i++)
            {
                if (output[i] >= 0) LocaleClose(output[i]);
                if (errors[i] >= 0) LocaleClose(errors[i]);
            }
            stdoutStream?.Dispose(); stderrStream?.Dispose();
            if (pid != 0)
            {
                LocaleKill(pid, 9);
                int ignored;
                while (LocaleWaitPid(pid, &ignored, 0) < 0 && Marshal.GetLastPInvokeError() == EINTR) { }
            }
        }
    }

    [DllImport("libc", EntryPoint = "gnu_get_libc_version")]
    private static extern byte* LocaleGlibcVersion();
    [DllImport("libc", EntryPoint = "pipe2", SetLastError = true)]
    private static extern int LocalePipe(int* descriptors, int flags);
    [DllImport("libc", EntryPoint = "close", SetLastError = true)]
    private static extern int LocaleClose(int descriptor);
    [DllImport("libc", EntryPoint = "posix_spawn_file_actions_init")]
    private static extern int LocaleActionsInit(LocaleSpawnActions* actions);
    [DllImport("libc", EntryPoint = "posix_spawn_file_actions_destroy")]
    private static extern int LocaleActionsDestroy(LocaleSpawnActions* actions);
    [DllImport("libc", EntryPoint = "posix_spawn_file_actions_addopen")]
    private static extern int LocaleActionsOpen(LocaleSpawnActions* actions, int descriptor, byte* path, int flags, uint mode);
    [DllImport("libc", EntryPoint = "posix_spawn_file_actions_adddup2")]
    private static extern int LocaleActionsDup(LocaleSpawnActions* actions, int descriptor, int destination);
    [DllImport("libc", EntryPoint = "posix_spawn_file_actions_addclose")]
    private static extern int LocaleActionsClose(LocaleSpawnActions* actions, int descriptor);
    [DllImport("libc", EntryPoint = "posix_spawn")]
    private static extern int LocaleSpawn(int* process, byte* executable, LocaleSpawnActions* actions, void* attributes, byte** argv, byte** envp);
    [DllImport("libc", EntryPoint = "waitpid", SetLastError = true)]
    private static extern int LocaleWaitPid(int process, int* status, int options);
    [DllImport("libc", EntryPoint = "kill", SetLastError = true)]
    private static extern int LocaleKill(int process, int signal);
}
