using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using CoreLibc = Managed.Database.ValkeyCore.Libc;

namespace Managed.Database;

/// <summary>Managed lifecycle and policy boundary; all entries require the owning runtime binding.</summary>
public static unsafe partial class ValkeyHost
{
    private sealed partial class HostState
    {
        internal int Lifecycle;
        internal bool ConfigurationInitialized, ModulesInitialized, ServerEntered, ShutdownFinished;
        internal nint Error;
    }

    private static readonly ConditionalWeakTable<ValkeyCore, HostState> owners = new();
    private static HostState For(ValkeyCore core) => owners.GetValue(core, static _ => new());
    public static int State(ValkeyCore core) => For(core).Lifecycle;
    public static int Port(ValkeyCore core) => State(core) == 2 ? core.Globals.server.port : 0;
    public static byte* LastError(ValkeyCore core) => (byte*)For(core).Error;

    private static int Failure(ValkeyCore core, string message)
    {
        var state = For(core);
        byte[] bytes = Encoding.UTF8.GetBytes(message + '\0');
        byte* memory = (byte*)CoreLibc.malloc(bytes.Length);
        if (memory == null) throw new OutOfMemoryException();
        bytes.CopyTo(new Span<byte>(memory, bytes.Length));
        CoreLibc.free((void*)state.Error);
        state.Error = (nint)memory;
        return -1;
    }

    private static string? Text(byte* value) => value == null ? null : Marshal.PtrToStringUTF8((nint)value);

    public static int ResolveStaticModuleSymbol(ValkeyCore core, void** output, void** handle, byte* symbol, byte* module)
    {
        if (output != null) *output = null;
        if (handle != null) *handle = null;
        if (output == null || handle == null || Text(module) != "lua") return -1;
        switch (Text(symbol))
        {
            case "ValkeyModule_OnLoad":
                *output = (void*)(delegate*<ValkeyCore, ValkeyCore.ValkeyModuleCtx*, ValkeyCore.serverObject**, int, int>)&LoadLua;
                return 0;
            case "ValkeyModule_OnUnload":
                *output = (void*)(delegate*<ValkeyCore, ValkeyCore.ValkeyModuleCtx*, int>)&UnloadLua;
                return 0;
            default: return -1;
        }
    }

    // A hosted instance cannot install process-global signal callbacks.
    public static int Sigaction(ValkeyCore core, int signal, ValkeyCore.__DotCcTags.sigaction* action, ValkeyCore.__DotCcTags.sigaction* previous)
    {
        CoreLibc.errno = CoreLibc.ENOTSUP;
        return -1;
    }

    public static delegate*<ValkeyCore, int, void> Signal(ValkeyCore core, int signal, delegate*<ValkeyCore, int, void> handler)
    {
        CoreLibc.errno = CoreLibc.ENOTSUP;
        return (delegate*<ValkeyCore, int, void>)(nint)(-1);
    }

    public static int Glob(ValkeyCore core, byte* pattern, int flags,
        delegate*<ValkeyCore, byte*, int, int> error, ValkeyCore.glob_t* result) =>
        CoreLibc.GlobManaged(core, pattern, flags, error, result);

    public static void GlobFree(ValkeyCore core, ValkeyCore.glob_t* result) => CoreLibc.globfree(result);

    private static int LoadLua(ValkeyCore core, ValkeyCore.ValkeyModuleCtx* context, ValkeyCore.serverObject** argv, int argc)
    {
        using var binding = core.__DotCcEnter();
        return core.ValkeyModule_OnLoad_lua(context, argv, argc);
    }

    private static int UnloadLua(ValkeyCore core, ValkeyCore.ValkeyModuleCtx* context)
    {
        using var binding = core.__DotCcEnter();
        return core.ValkeyModule_OnUnload_lua(context);
    }

    private static void OutOfMemory(ValkeyCore core, ulong size)
    {
        using var binding = core.__DotCcEnter();
        core.serverOutOfMemoryHandler(size);
    }

    public static int ConfigAllowed(ValkeyCore core, byte* name, byte* value, int startup)
    {
        string? option = Text(name)?.ToLowerInvariant(), setting = Text(value);
        // Admission is independent of the qualification inventory. Let upstream
        // validate names/values; only prevent activation of fork-dependent work.
        bool requiresFork = option switch
        {
            "save" => !string.IsNullOrWhiteSpace(setting),
            "auto-aof-rewrite-percentage" => long.TryParse(setting, NumberStyles.Integer,
                CultureInfo.InvariantCulture, out long percentage) && percentage > 0,
            "daemonize" => string.Equals(setting, "yes", StringComparison.OrdinalIgnoreCase),
            "replicaof" or "slaveof" => true,
            "appendonly" => startup == 0 && core.Globals.server.aof_state == ValkeyCore.AOF_OFF
                && string.Equals(setting, "yes", StringComparison.OrdinalIgnoreCase),
            _ => false
        };
        return requiresFork ? 0 : 1;
    }

    private static bool ValidateOptions(ValkeyCore core, byte* options)
    {
        if (options == null || *options == 0) return true;
        foreach (string line in Text(options)!.Split('\n'))
        {
            string trimmed = line.Trim(' ', '\t', '\r');
            if (trimmed.Length == 0 || trimmed[0] == '#') continue;
            byte[] bytes = Encoding.UTF8.GetBytes(trimmed + '\0');
            fixed (byte* input = bytes)
            {
                int count = 0;
                byte** args = core.sdssplitargs(input, &count);
                try
                {
                    // Malformed or multi-argument directives remain upstream's
                    // responsibility; do not impose a second configuration grammar.
                    if (args != null && count >= 2 && ConfigAllowed(core, args[0], args[1], 1) == 0)
                        return false;
                }
                finally { if (args != null) core.sdsfreesplitres(args, count); }
            }
        }
        return true;
    }

    public static int CommandAllowed(ValkeyCore core, ValkeyCore.client* client)
    {
        if (client == null || client->cmd == null) return 1;
        // Use the resolved canonical name so command aliases cannot bypass a
        // fork restriction. Unqualified commands otherwise reach upstream.
        string? command = Text(client->cmd->fullname)?.ToLowerInvariant();
        if (command != null && ForkDependentCommands.Contains(command)) return 0;
        if (command == "script|debug" && client->argc == 3 &&
            string.Equals(Text((byte*)core.objectGetVal(client->argv[2])), "yes", StringComparison.OrdinalIgnoreCase))
            return 0; // Synchronous debugging and disabling the debugger do not fork.
        if (command == "config|set" && (client->argc & 1) == 0)
            for (int i = 2; i < client->argc; i += 2)
                if (ConfigAllowed(core, (byte*)core.objectGetVal(client->argv[i]),
                    (byte*)core.objectGetVal(client->argv[i + 1]), 0) == 0) return 0;
        return 1;
    }

    public static int Start(ValkeyCore core, byte* options)
    {
        var state = For(core);
        if (state.Lifecycle != 0) return Failure(core, "A Valkey owner can only be started once");
        state.Lifecycle = 1;
        core.zmalloc_set_oom_handler(&OutOfMemory);
        if (!ValidateOptions(core, options)) return Failure(core, "Startup configuration requires fork, which is unavailable in the managed runtime");
        // Seeds are per owner; Valkey's own dictionaries and PRNG remain upstream.
        ulong entropy = BitConverter.ToUInt64(System.Security.Cryptography.RandomNumberGenerator.GetBytes(8));
        CoreLibc.srand((uint)entropy);
        CoreLibc.srandom((uint)(entropy >> 32));
        core.init_genrand64(entropy);
        core.crc64_init();
        byte* seed = stackalloc byte[16];
        core.getRandomBytes(seed, 16);
        core.dictSetHashFunctionSeed(seed);
        core.hashtableSetHashFunctionSeed(seed);
        core.initServerConfig();
        state.ConfigurationInitialized = true;
        ref var server = ref core.Globals.server;
        fixed (int* pipe = server.module_pipe) pipe[0] = pipe[1] = -1;
        server.pid = CoreLibc.getpid();
        fixed (byte* executable = "dotcc-managed-valkey\0"u8) server.executable = core.zstrdup(executable);
        server.exec_argv = (byte**)core.zcalloc_num(1, (ulong)sizeof(byte*));
        core.ACLInit();
        core.moduleInitModulesSystem();
        state.ModulesInitialized = true;
        if (core.connTypeInitialize() != 0) return Failure(core, "Connection type initialization failed");
        byte[] config = Encoding.UTF8.GetBytes(DefaultConfiguration + (Text(options) ?? "") + '\0');
        fixed (byte* configuration = config) core.loadServerConfig(null, 0, configuration);
        // Includes and configuration aliases are parsed by upstream. Check the
        // resulting state too, before initialization can start background work.
        if (server.saveparamslen > 0 || server.aof_rewrite_perc > 0 ||
            server.daemonize != 0 || server.primary_host != null)
            return Failure(core, "Startup configuration requires fork, which is unavailable in the managed runtime");
        if (server.port is < 1 or > 65535) return Failure(core, "A TCP listener port is required");
        state.ServerEntered = true;
        core.initServer();
        if (server.cluster_enabled != 0) core.clusterInit();
        core.moduleInitModulesSystemLast();
        core.moduleLoadFromQueue();
        core.ACLLoadUsersAtStartup();
        core.initListeners();
        if (server.cluster_enabled != 0) core.clusterInitLast();
        fixed (byte* lua = "lua\0"u8)
            if (core.moduleLoadStatic(lua, null, 0, 0) != 0) return Failure(core, "Static Lua initialization failed");
        server.lua_insecure_api_current = server.lua_enable_insecure_api;
        core.InitServerLast();
        core.aofLoadManifestFromDisk();
        core.loadDataFromDisk();
        core.aofOpenIfNeededOnServerStart();
        core.aofDelHistoryFiles();
        state.Lifecycle = 2;
        return 0;
    }

    private const string DefaultConfiguration =
        "bind 127.0.0.1\nport 6379\nprotected-mode yes\nsave \"\"\n" +
        "auto-aof-rewrite-percentage 0\nio-threads 1\nappendonly no\nappendfsync always\n" +
        "daemonize no\nsupervised no\nset-proc-title no\nsyslog-enabled no\n" +
        "crash-log-enabled no\ncrash-memcheck-enabled no\nwatchdog-period 0\nmaxclients 512\n" +
        "enable-debug-command no\nenable-module-command no\n";

    public static int ProcessEvents(ValkeyCore core) => State(core) == 2
        ? core.aeProcessEvents(core.Globals.server.el, ValkeyCore.AE_ALL_EVENTS | ValkeyCore.AE_DONT_WAIT |
            ValkeyCore.AE_CALL_BEFORE_SLEEP | ValkeyCore.AE_CALL_AFTER_SLEEP)
        : Failure(core, "Valkey is not ready to process events");

    public static void ShutdownCompleted(ValkeyCore core)
    {
        var state = For(core);
        state.ShutdownFinished = true;
        state.Lifecycle = 3;
        if (core.Globals.server.el != null) core.aeStop(core.Globals.server.el);
    }

    public static int Stop(ValkeyCore core, int flags)
    {
        if (State(core) is 3 or 4) return 0;
        if (State(core) != 2) return Failure(core, "Valkey startup did not complete");
        if (core.prepareForShutdown(null, flags) != 0)
            return Failure(core, "Upstream shutdown did not complete; server remains running");
        ShutdownCompleted(core);
        return 0;
    }

    public static int Cleanup(ValkeyCore core, int abandon)
    {
        var state = For(core);
        if (state.Lifecycle == 4) return 0;
        if (state.Lifecycle == 2 && abandon == 0) return Failure(core, "Stop must succeed before cleanup");
        ref var server = ref core.Globals.server;
        if (state.ServerEntered && server.el != null) core.aeStop(server.el);
        if (state.ServerEntered && !state.ShutdownFinished) core.closeListeningSockets(0);
        if (state.ModulesInitialized && !state.ShutdownFinished && abandon == 0) core.moduleUnloadAllModules();
        StopIoWorkers(core);
        if (BioStop(core, abandon) != 0) return Failure(core, "Background workers have not all joined");
        if (state.ServerEntered && server.el != null)
        {
            core.aeDeleteEventLoop(server.el);
            server.el = null;
        }
        if (state.ConfigurationInitialized)
        {
            fixed (int* pipe = server.module_pipe)
            {
                if (pipe[0] >= 0) CoreLibc.close(pipe[0]);
                if (pipe[1] >= 0) CoreLibc.close(pipe[1]);
                pipe[0] = pipe[1] = -1;
            }
        }
        state.Lifecycle = 4;
        return 0;
    }
}
