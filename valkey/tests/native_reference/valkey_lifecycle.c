/* The algorithms, protocol dispatch, Lua engine, persistence and shutdown
 * decisions below are upstream Valkey. This boundary supplies lifecycle and
 * capability policy for an instance owned by a managed RuntimeContext. */
#include "server.h"
#include "connection.h"
#include "bio.h"
#include "mt19937-64.h"
#include "crc64.h"
#include "valkey_host.h"
#include <sys/time.h>

/* CLI-local declarations used without executing CLI process setup. */
void initServerConfig(void);
void initServer(void);
void InitServerLast(void);
void initListeners(void);
void loadDataFromDisk(void);
void moduleInitModulesSystem(void);
void moduleInitModulesSystemLast(void);
int moduleLoadStatic(const char *name, void **argv, int argc, int is_loadex);
void moduleUnloadAllModules(void);
void serverOutOfMemoryHandler(size_t size);

static int managed_state;
static int managed_config_initialized;
static int managed_modules_initialized;
static int managed_server_entered;
static int managed_shutdown_finished;
static const char *managed_error;

static int managedFailure(const char *message) {
    managed_error = message;
    return C_ERR;
}

const char *valkeyManagedLastError(void) { return managed_error; }
int valkeyManagedState(void) { return managed_state; }
int valkeyManagedPort(void) { return managed_state == 2 ? server.port : 0; }

/* Check every requested option before the upstream parser can change paths,
 * load a module, create a process, or turn on a deferred subsystem. */
int valkeyManagedConfigAllowed(const char *name, const char *value, int startup) {
    if (!name || !value) return 0;
    if (!strcasecmp(name, "save")) return value[0] == '\0';
    if (!strcasecmp(name, "auto-aof-rewrite-percentage") || !strcasecmp(name, "watchdog-period") ||
        !strcasecmp(name, "tls-port")) return !strcmp(value, "0");
    if (!strcasecmp(name, "io-threads")) return !strcmp(value, "1");
    if (!strcasecmp(name, "daemonize") || !strcasecmp(name, "cluster-enabled") ||
        !strcasecmp(name, "set-proc-title") || !strcasecmp(name, "syslog-enabled") ||
        !strcasecmp(name, "crash-log-enabled") || !strcasecmp(name, "crash-memcheck-enabled"))
        return !strcasecmp(value, "no");
    if (!strcasecmp(name, "supervised") || !strcasecmp(name, "enable-debug-command") ||
        !strcasecmp(name, "enable-module-command")) return !strcasecmp(value, "no");
    if (!strcasecmp(name, "appendonly"))
        return !strcasecmp(value, "no") || (startup && !strcasecmp(value, "yes"));
    if (!strcasecmp(name, "maxclients")) {
        char *end;
        long count = strtol(value, &end, 10);
        return *value && !*end && count > 0 && count <= 512;
    }
    /* These names have ordinary per-server setters. Unknown options are denied
     * rather than permitting unreviewed process or filesystem side effects. */
    const char *allowed[] = {
        "port", "bind", "protected-mode", "requirepass", "databases", "maxmemory", "maxmemory-policy",
        "timeout", "tcp-keepalive", "tcp-backlog", "hz", "loglevel", "appendfsync", "no-appendfsync-on-rewrite",
        "dbfilename", "appendfilename", "appenddirname", "rdbchecksum", "rdbcompression",
        "stop-writes-on-bgsave-error", "activerehashing", "lazyfree-lazy-eviction", "lazyfree-lazy-expire",
        "lazyfree-lazy-server-del", "lazyfree-lazy-user-del", "lazyfree-lazy-user-flush", "lua-time-limit",
        "busy-reply-threshold", "notify-keyspace-events", "slowlog-log-slower-than", "slowlog-max-len",
        "client-output-buffer-limit", "maxmemory-clients", NULL
    };
    for (int i = 0; allowed[i]; ++i)
        if (!strcasecmp(name, allowed[i])) return 1;
    return 0;
}

static int managedValidateOptions(const char *options) {
    if (!*options) return C_OK;
    int count;
    sds *lines = sdssplitlen(options, strlen(options), "\n", 1, &count);
    if (!lines) return managedFailure("Cannot parse managed startup options");
    int valid = 1;
    for (int i = 0; i < count && valid; ++i) {
        lines[i] = sdstrim(lines[i], " \t\r");
        if (!lines[i][0] || lines[i][0] == '#') continue;
        int argc;
        sds *argv = sdssplitargs(lines[i], &argc);
        /* The initial API accepts one value per option. Quote multiword values
         * so the upstream parser still performs its normal validation. */
        valid = argv && argc == 2 && valkeyManagedConfigAllowed(argv[0], argv[1], 1);
        if (argv) sdsfreesplitres(argv, argc);
    }
    sdsfreesplitres(lines, count);
    return valid ? C_OK : managedFailure("Startup option is outside the managed profile");
}

int valkeyManagedCommandAllowed(client *c) {
    const char *name = objectGetVal(c->argv[0]);
    const char *denied[] = {"bgsave", "bgrewriteaof", "replicaof", "slaveof", "replconf", "psync", "sync",
                            "failover", "cluster", "slot-migration", "debug", NULL};
    for (int i = 0; denied[i]; ++i)
        if (!strcasecmp(name, denied[i])) return 0;
    if (!strcasecmp(name, "module"))
        return c->argc == 2 && !strcasecmp(objectGetVal(c->argv[1]), "list");
    if (!strcasecmp(name, "script") && c->argc > 1 && !strcasecmp(objectGetVal(c->argv[1]), "debug")) return 0;
    if (!strcasecmp(name, "config") && c->argc > 1) {
        const char *operation = objectGetVal(c->argv[1]);
        if (!strcasecmp(operation, "rewrite")) return 0;
        if (!strcasecmp(operation, "set")) {
            if (c->argc & 1) return 1; /* upstream reports the arity error */
            for (int i = 2; i < c->argc; i += 2)
                if (!valkeyManagedConfigAllowed(objectGetVal(c->argv[i]), objectGetVal(c->argv[i + 1]), 0)) return 0;
        }
    }
    return 1;
}

int valkeyManagedStart(const char *options) {
    if (managed_state != 0) return managedFailure("A Valkey owner can only be started once");
    managed_state = 1;
    managed_error = NULL;
    zmalloc_set_oom_handler(serverOutOfMemoryHandler);
    if (!options) options = "";
    if (managedValidateOptions(options) != C_OK) return C_ERR;

    struct timeval tv;
    gettimeofday(&tv, NULL);
    srand(time(NULL) ^ getpid() ^ tv.tv_usec);
    srandom(time(NULL) ^ getpid() ^ tv.tv_usec);
    init_genrand64(((long long)tv.tv_sec * 1000000 + tv.tv_usec) ^ getpid());
    crc64_init();
    uint8_t seed[16];
    getRandomBytes(seed, sizeof(seed));
    dictSetHashFunctionSeed(seed);
    hashtableSetHashFunctionSeed(seed);
    initServerConfig();
    managed_config_initialized = 1;
    server.module_pipe[0] = server.module_pipe[1] = -1;
    server.pid = getpid();
    server.executable = zstrdup("dotcc-managed-valkey");
    server.exec_argv = zcalloc(sizeof(char *));
    ACLInit();
    moduleInitModulesSystem();
    managed_modules_initialized = 1;
    if (connTypeInitialize() != C_OK) return managedFailure("Connection type initialization failed");

    sds config = sdsnew(
        "bind 127.0.0.1\nport 6379\nprotected-mode yes\nsave \"\"\n"
        "auto-aof-rewrite-percentage 0\nio-threads 1\nappendonly no\nappendfsync always\n"
        "daemonize no\nsupervised no\nset-proc-title no\nsyslog-enabled no\n"
        "crash-log-enabled no\ncrash-memcheck-enabled no\nwatchdog-period 0\nmaxclients 512\n"
        "enable-debug-command no\nenable-module-command no\n");
    config = sdscat(config, options);
    loadServerConfig(NULL, 0, config);
    sdsfree(config);
    if (server.port < 1 || server.port > 65535) return managedFailure("A TCP listener port is required");
    managed_server_entered = 1;
    initServer();
    moduleInitModulesSystemLast();
    ACLLoadUsersAtStartup();
    initListeners();
    if (moduleLoadStatic("lua", NULL, 0, 0) != C_OK) return managedFailure("Static Lua initialization failed");
    server.lua_insecure_api_current = server.lua_enable_insecure_api;
    InitServerLast();
    aofLoadManifestFromDisk();
    loadDataFromDisk();
    aofOpenIfNeededOnServerStart();
    aofDelHistoryFiles();
    managed_state = 2;
    return C_OK;
}

int valkeyManagedProcessEvents(void) {
    if (managed_state != 2) return managedFailure("Valkey is not ready to process events");
    return aeProcessEvents(server.el, AE_ALL_EVENTS | AE_DONT_WAIT | AE_CALL_BEFORE_SLEEP | AE_CALL_AFTER_SLEEP);
}

void valkeyManagedShutdownCompleted(void) {
    managed_shutdown_finished = 1;
    managed_state = 3;
    if (server.el) aeStop(server.el);
}

int valkeyManagedStop(int flags) {
    if (managed_state == 3 || managed_state == 4) return C_OK;
    if (managed_state != 2) return managedFailure("Valkey startup did not complete");
    if (prepareForShutdown(NULL, flags) != C_OK) return managedFailure("Upstream shutdown did not complete; server remains running");
    valkeyManagedShutdownCompleted();
    return C_OK;
}

int valkeyManagedCleanup(int abandon) {
    if (managed_state == 4) return C_OK;
    if (managed_state == 2 && !abandon) return managedFailure("Stop must succeed before cleanup");
    if (managed_server_entered && server.el) aeStop(server.el);
    if (managed_server_entered && !managed_shutdown_finished) closeListeningSockets(0);
    /* Normal upstream shutdown already unloaded modules. On a failed startup,
     * unload only if module initialization completed. Fault cleanup avoids
     * invoking arbitrary module code while unwinding a terminal failure. */
    if (managed_modules_initialized && !managed_shutdown_finished && !abandon) moduleUnloadAllModules();
    if (bioManagedStop(abandon) != C_OK) return managedFailure("Background workers have not all joined");
    if (managed_server_entered && server.el) {
        aeDeleteEventLoop(server.el);
        server.el = NULL;
    }
    if (managed_config_initialized) {
        if (server.module_pipe[0] >= 0) close(server.module_pipe[0]);
        if (server.module_pipe[1] >= 0) close(server.module_pipe[1]);
        server.module_pipe[0] = server.module_pipe[1] = -1;
    }
    /* Remaining clients/database allocations and descriptors belong to the
     * managed runtime arena. Its disposal is permitted only after this join. */
    managed_state = 4;
    return C_OK;
}
