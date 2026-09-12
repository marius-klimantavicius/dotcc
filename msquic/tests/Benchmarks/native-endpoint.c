/* Independent native benchmark only. Never linked into the managed product. */
#define QUIC_API_ENABLE_PREVIEW_FEATURES 1
#include <msquic.h>
#include <arpa/inet.h>
#include <errno.h>
#include <pthread.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/resource.h>
#include <time.h>
#include <unistd.h>

#define READY_MARKER 0xa7
#define MAX_TRANSFERS 20
static const QUIC_API_TABLE *api;
static HQUIC configuration, connection;
static pthread_mutex_t gate = PTHREAD_MUTEX_INITIALIZER;
static pthread_cond_t changed;
static int failed, connected, connection_closed;
static QUIC_STATUS transport_status;
static uint64_t transport_error, peer_error;
static QUIC_HANDSHAKE_INFO handshake;
static uint32_t version;
static struct {
    int server, bytes, warmups, iterations, chunk, cipher;
    const char *certificate, *key_or_name, *ip, *ready_file;
    uint16_t port;
} options;

typedef struct Transfer {
    HQUIC stream;
    int index, opened, started, send_pending, marker_received, fin_received, write_finished;
    uint64_t wire_received;
} Transfer;
static Transfer transfers[MAX_TRANSFERS];
static int next_transfer;

typedef struct Resources { uint64_t cpu_us, rss_bytes, peak_rss_bytes; } Resources;

static uint64_t now_us(void) {
    struct timespec value;
    clock_gettime(CLOCK_MONOTONIC, &value);
    return (uint64_t)value.tv_sec * 1000000 + (uint64_t)value.tv_nsec / 1000;
}
static Resources sample_resources(void) {
    struct rusage usage;
    Resources result = {0};
    if (getrusage(RUSAGE_SELF, &usage) == 0) {
        result.cpu_us = (uint64_t)(usage.ru_utime.tv_sec + usage.ru_stime.tv_sec) * 1000000 +
            (uint64_t)usage.ru_utime.tv_usec + (uint64_t)usage.ru_stime.tv_usec;
        result.peak_rss_bytes = (uint64_t)usage.ru_maxrss * 1024; /* Linux reports KiB. */
    }
    FILE *file = fopen("/proc/self/statm", "r");
    unsigned long total, resident;
    if (file) {
        if (fscanf(file, "%lu %lu", &total, &resident) == 2) result.rss_bytes = (uint64_t)resident * (uint64_t)sysconf(_SC_PAGESIZE);
        fclose(file);
    }
    return result;
}
static void print_resources(const char *phase, int index, Resources value) {
    printf("{\"metric\":\"memory\",\"phase\":\"%s\",\"index\":%d,\"rss_bytes\":%llu,\"peak_rss_bytes\":%llu,\"managed_heap_bytes\":null,\"process_cpu_us\":%llu}\n",
        phase, index, (unsigned long long)value.rss_bytes, (unsigned long long)value.peak_rss_bytes, (unsigned long long)value.cpu_us);
}
static void fail_locked(const char *message) {
    if (!failed) fprintf(stderr, "%s\n", message);
    failed = 1; pthread_cond_broadcast(&changed);
}
static int check(QUIC_STATUS status, const char *operation) {
    if (QUIC_SUCCEEDED(status)) return 1;
    pthread_mutex_lock(&gate);
    fprintf(stderr, "%s status=%u\n", operation, (unsigned)status);
    fail_locked(operation);
    pthread_mutex_unlock(&gate);
    return 0;
}
static int has_failed(void) {
    pthread_mutex_lock(&gate); int result = failed; pthread_mutex_unlock(&gate); return result;
}
static int wait_flag(int *flag, int value, int ignore_failure) {
    struct timespec deadline;
    clock_gettime(CLOCK_MONOTONIC, &deadline); deadline.tv_sec += 120;
    pthread_mutex_lock(&gate);
    while (*flag != value && (!failed || ignore_failure)) {
        int error = pthread_cond_timedwait(&changed, &gate, &deadline);
        if (error) { fail_locked(error == ETIMEDOUT ? "Benchmark operation timed out" : "Condition wait failed"); break; }
    }
    int result = *flag == value;
    pthread_mutex_unlock(&gate);
    return result;
}
static unsigned char pattern(uint64_t offset, int server, int index) {
    return (unsigned char)(offset * 31 + (server ? 83 : 17) + index * 7);
}
static QUIC_STATUS QUIC_API stream_callback(HQUIC handle, void *context, QUIC_STREAM_EVENT *event) {
    (void)handle;
    Transfer *transfer = context;
    pthread_mutex_lock(&gate);
    switch (event->Type) {
    case QUIC_STREAM_EVENT_START_COMPLETE:
        if (QUIC_FAILED(event->START_COMPLETE.Status)) fail_locked("Stream start failed");
        transfer->started = 1;
        break;
    case QUIC_STREAM_EVENT_RECEIVE:
        if (event->RECEIVE.AbsoluteOffset != transfer->wire_received) fail_locked("Receive offset mismatch");
        for (uint32_t b = 0; b < event->RECEIVE.BufferCount && !failed; b++) {
            const QUIC_BUFFER *buffer = &event->RECEIVE.Buffers[b];
            for (uint32_t i = 0; i < buffer->Length; i++) {
                uint64_t offset = transfer->wire_received;
                if (offset > (uint64_t)options.bytes || buffer->Buffer[i] != (offset == 0 ? READY_MARKER : pattern(offset - 1, !options.server, transfer->index))) {
                    fail_locked("Payload or marker mismatch"); break;
                }
                if (!offset) transfer->marker_received = 1;
                transfer->wire_received++;
            }
        }
        break;
    case QUIC_STREAM_EVENT_SEND_COMPLETE:
        free(event->SEND_COMPLETE.ClientContext);
        transfer->send_pending = 0;
        if (event->SEND_COMPLETE.Canceled) fail_locked("Send canceled");
        break;
    case QUIC_STREAM_EVENT_PEER_SEND_SHUTDOWN:
        if (transfer->wire_received != (uint64_t)options.bytes + 1) fail_locked("Incomplete payload at FIN");
        transfer->fin_received = 1;
        break;
    case QUIC_STREAM_EVENT_SEND_SHUTDOWN_COMPLETE:
        if (!event->SEND_SHUTDOWN_COMPLETE.Graceful) fail_locked("Write shutdown was not graceful");
        transfer->write_finished = 1;
        break;
    case QUIC_STREAM_EVENT_PEER_SEND_ABORTED:
    case QUIC_STREAM_EVENT_PEER_RECEIVE_ABORTED:
        fail_locked("Peer aborted benchmark stream"); break;
    default: break;
    }
    pthread_cond_broadcast(&changed);
    pthread_mutex_unlock(&gate);
    return QUIC_STATUS_SUCCESS;
}
static QUIC_STATUS QUIC_API connection_callback(HQUIC handle, void *context, QUIC_CONNECTION_EVENT *event) {
    (void)context;
    if (event->Type == QUIC_CONNECTION_EVENT_CONNECTED) {
        uint32_t length = sizeof(handshake);
        check(api->GetParam(handle, QUIC_PARAM_TLS_HANDSHAKE_INFO, &length, &handshake), "handshake information");
        length = sizeof(version);
        check(api->GetParam(handle, QUIC_PARAM_CONN_QUIC_VERSION, &length, &version), "QUIC version");
    }
    pthread_mutex_lock(&gate);
    switch (event->Type) {
    case QUIC_CONNECTION_EVENT_CONNECTED:
        if (event->CONNECTED.NegotiatedAlpnLength != 14 || memcmp(event->CONNECTED.NegotiatedAlpn, "dotcc-bench-v1", 14) ||
            version != 1 || handshake.TlsGroup != QUIC_TLS_GROUP_SECP256R1 ||
            handshake.CipherSuite != (options.cipher == 128 ? QUIC_CIPHER_SUITE_TLS_AES_128_GCM_SHA256 : QUIC_CIPHER_SUITE_TLS_AES_256_GCM_SHA384))
            fail_locked("Negotiated profile mismatch");
        connected = 1;
        break;
    case QUIC_CONNECTION_EVENT_PEER_STREAM_STARTED: {
        if (!options.server || next_transfer >= options.warmups + options.iterations) {
            fail_locked("Unexpected incoming stream");
            pthread_mutex_unlock(&gate);
            return QUIC_STATUS_CONNECTION_REFUSED;
        }
        Transfer *transfer = &transfers[next_transfer++];
        transfer->stream = event->PEER_STREAM_STARTED.Stream;
        // SetCallbackHandler only stores the handler/context. The callback is
        // installed before publishing this stream to the producer thread.
        api->SetCallbackHandler(transfer->stream, (void *)stream_callback, transfer);
        transfer->started = 1; transfer->opened = 1;
        break;
    }
    case QUIC_CONNECTION_EVENT_SHUTDOWN_INITIATED_BY_TRANSPORT:
        transport_status = event->SHUTDOWN_INITIATED_BY_TRANSPORT.Status;
        transport_error = event->SHUTDOWN_INITIATED_BY_TRANSPORT.ErrorCode;
        if (QUIC_FAILED(transport_status)) fail_locked("Transport shutdown");
        break;
    case QUIC_CONNECTION_EVENT_SHUTDOWN_INITIATED_BY_PEER:
        peer_error = event->SHUTDOWN_INITIATED_BY_PEER.ErrorCode;
        if (peer_error) fail_locked("Peer application error");
        break;
    case QUIC_CONNECTION_EVENT_SHUTDOWN_COMPLETE: connection_closed = 1; break;
    default: break;
    }
    pthread_cond_broadcast(&changed);
    pthread_mutex_unlock(&gate);
    return QUIC_STATUS_SUCCESS;
}
static QUIC_STATUS QUIC_API listener_callback(HQUIC handle, void *context, QUIC_LISTENER_EVENT *event) {
    (void)handle; (void)context;
    if (event->Type != QUIC_LISTENER_EVENT_NEW_CONNECTION) return QUIC_STATUS_SUCCESS;
    pthread_mutex_lock(&gate);
    if (connection) { pthread_mutex_unlock(&gate); return QUIC_STATUS_CONNECTION_REFUSED; }
    connection = event->NEW_CONNECTION.Connection;
    pthread_mutex_unlock(&gate);
    api->SetCallbackHandler(connection, (void *)connection_callback, NULL);
    QUIC_STATUS status = api->ConnectionSetConfiguration(connection, configuration);
    if (QUIC_FAILED(status)) {
        pthread_mutex_lock(&gate); connection = NULL; fail_locked("Connection configuration rejected"); pthread_mutex_unlock(&gate);
    }
    return status; /* Failure leaves raw ownership with the core. */
}
static int send_buffer(Transfer *transfer, uint8_t *data, uint32_t length, int fin) {
    QUIC_BUFFER *buffer = malloc(sizeof(*buffer));
    if (!buffer) { check(QUIC_STATUS_OUT_OF_MEMORY, "send descriptor allocation"); return 0; }
    *buffer = (QUIC_BUFFER){length, data};
    pthread_mutex_lock(&gate); transfer->send_pending = 1; pthread_mutex_unlock(&gate);
    QUIC_STATUS status = api->StreamSend(transfer->stream, buffer, 1, fin ? QUIC_SEND_FLAG_FIN : QUIC_SEND_FLAG_NONE, buffer);
    if (QUIC_FAILED(status)) {
        free(buffer);
        pthread_mutex_lock(&gate); transfer->send_pending = 0; pthread_mutex_unlock(&gate);
        check(status, "StreamSend"); return 0;
    }
    return wait_flag(&transfer->send_pending, 0, 0) && !has_failed();
}
static int transfer_once(Transfer *transfer) {
    uint8_t marker = READY_MARKER;
    uint8_t *payload = malloc((size_t)options.bytes);
    if (!payload) return check(QUIC_STATUS_OUT_OF_MEMORY, "payload allocation");
    for (int i = 0; i < options.bytes; i++) payload[i] = pattern((uint64_t)i, options.server, transfer->index);
    uint64_t open_begin = now_us();
    int okay;
    if (options.server) okay = wait_flag(&transfer->opened, 1, 0);
    else {
        okay = check(api->StreamOpen(connection, QUIC_STREAM_OPEN_FLAG_NONE, stream_callback, transfer, &transfer->stream), "StreamOpen");
        if (okay) okay = check(api->StreamStart(transfer->stream, QUIC_STREAM_START_FLAG_IMMEDIATE), "StreamStart") && wait_flag(&transfer->started, 1, 0);
    }
    uint64_t open_us = now_us() - open_begin;
    if (okay && !has_failed()) {
        Resources before = sample_resources(); print_resources("load_begin", transfer->index, before);
        uint64_t begin = now_us();
        okay = send_buffer(transfer, &marker, 1, 0) && wait_flag(&transfer->marker_received, 1, 0);
        for (int offset = 0; offset < options.bytes && okay; offset += options.chunk) {
            int count = options.bytes - offset;
            if (count > options.chunk) count = options.chunk;
            okay = send_buffer(transfer, payload + offset, (uint32_t)count, offset + count == options.bytes);
        }
        okay = okay && wait_flag(&transfer->fin_received, 1, 0) && wait_flag(&transfer->write_finished, 1, 0) && !has_failed();
        uint64_t elapsed = now_us() - begin;
        Resources after = sample_resources();
        if (okay) {
            printf("{\"metric\":\"transfer\",\"index\":%d,\"warmup\":%s,\"payload_sent\":%d,\"payload_received\":%d,\"fin_received\":true,\"wall_us\":%llu,\"stream_open_or_accept_us\":%llu,\"cpu_us\":%llu,\"managed_allocated_bytes\":null}\n",
                transfer->index, transfer->index < options.warmups ? "true" : "false", options.bytes, options.bytes,
                (unsigned long long)elapsed, (unsigned long long)open_us, (unsigned long long)(after.cpu_us - before.cpu_us));
            print_resources("load_end", transfer->index, after);
        }
    }
    if (!okay || has_failed()) api->ConnectionShutdown(connection, QUIC_CONNECTION_SHUTDOWN_FLAG_NONE, 1);
    if (transfer->stream) { api->StreamClose(transfer->stream); transfer->stream = NULL; }
    /* StreamClose drains SEND_COMPLETE; borrowed payload cannot outlive here. */
    free(payload);
    return okay && !has_failed();
}
static int number(const char *text, long minimum, long maximum, int *value) {
    char *end; errno = 0; long parsed = strtol(text, &end, 10);
    if (!*text || *end || errno == ERANGE || parsed < minimum || parsed > maximum) return 0;
    *value = (int)parsed; return 1;
}
int main(int argc, char **argv) {
    int port, pipeline;
    if (argc != 13 || (strcmp(argv[1], "client") && strcmp(argv[1], "server"))) {
        fprintf(stderr, "endpoint client|server certificate-or-root key-or-server-name ip port ready-file bytes warmups iterations chunk-bytes pipeline cipher128|256\n"); return 2;
    }
    options.server = !strcmp(argv[1], "server");
    if (!number(argv[5], options.server ? 0 : 1, 65535, &port) || !number(argv[7], 16777216, 268435456, &options.bytes) ||
        !number(argv[8], 1, 4, &options.warmups) || !number(argv[9], 1, 16, &options.iterations) ||
        !number(argv[10], 4096, 4194304, &options.chunk) || !number(argv[11], 1, 1, &pipeline) ||
        !number(argv[12], 128, 256, &options.cipher) || (options.cipher != 128 && options.cipher != 256)) return 2;
    options.port = (uint16_t)port; options.certificate = argv[2]; options.key_or_name = argv[3]; options.ip = argv[4]; options.ready_file = argv[6];
    for (int i = 0; i < MAX_TRANSFERS; i++) transfers[i].index = i;
    pthread_condattr_t attributes; pthread_condattr_init(&attributes); pthread_condattr_setclock(&attributes, CLOCK_MONOTONIC);
    pthread_cond_init(&changed, &attributes); pthread_condattr_destroy(&attributes);
    printf("{\"metric\":\"configuration\",\"role\":\"%s\",\"bytes\":%d,\"warmups\":%d,\"iterations\":%d,\"chunk_bytes\":%d,\"pipeline\":1,\"transport_workers\":1,\"stream_window\":1048576,\"connection_window\":8388608,\"send_buffering\":false,\"pacing\":true,\"ecn\":false,\"encryption_offload\":false,\"alpn\":\"dotcc-bench-v1\",\"cipher\":%d}\n",
        options.server ? "server" : "client", options.bytes, options.warmups, options.iterations, options.chunk, options.cipher == 128 ? 0x1301 : 0x1302);
    HQUIC registration = NULL, listener = NULL;
    uint64_t shutdown_us = 0, disposal_begin, disposal_us;
    int shutdown_measured = 0;
    if (!check(MsQuicOpen2(&api), "MsQuicOpen2")) goto done;
    char revision[64] = {0}; uint32_t revision_size = sizeof(revision), provider_size = sizeof(QUIC_TLS_PROVIDER);
    QUIC_TLS_PROVIDER provider;
    if (!check(api->GetParam(NULL, QUIC_PARAM_GLOBAL_LIBRARY_GIT_HASH, &revision_size, revision), "source revision") ||
        !check(api->GetParam(NULL, QUIC_PARAM_GLOBAL_TLS_PROVIDER, &provider_size, &provider), "TLS provider")) goto done;
    revision[sizeof(revision) - 1] = '\0';
    for (size_t i = 0; revision[i]; i++) {
        if (!((revision[i] >= '0' && revision[i] <= '9') || (revision[i] >= 'a' && revision[i] <= 'z') ||
            (revision[i] >= 'A' && revision[i] <= 'Z') || revision[i] == '-' || revision[i] == '_')) {
            check(QUIC_STATUS_INVALID_PARAMETER, "unexpected revision text"); goto done;
        }
    }
    const char *required_revision = getenv("DOTCC_REQUIRED_SOURCE_REVISION");
    if (required_revision && strcmp(required_revision, revision)) { check(QUIC_STATUS_INVALID_PARAMETER, "source revision mismatch"); goto done; }
    printf("{\"metric\":\"identity\",\"implementation\":\"native\",\"source_revision\":\"%s\",\"tls_provider_id\":%u}\n", revision, (unsigned)provider);
    QUIC_GLOBAL_EXECUTION_CONFIG execution = {0};
    execution.Flags = QUIC_GLOBAL_EXECUTION_CONFIG_FLAG_NO_IDEAL_PROC;
    execution.ProcessorCount = 1; execution.ProcessorList[0] = 0;
    if (!check(api->SetParam(NULL, QUIC_PARAM_GLOBAL_EXECUTION_CONFIG, sizeof(execution), &execution), "one-worker execution policy")) goto done;
    uint32_t selected = 1;
    QUIC_VERSION_SETTINGS versions = {&selected, &selected, &selected, 1, 1, 1};
    if (!check(api->SetParam(NULL, QUIC_PARAM_GLOBAL_VERSION_SETTINGS, sizeof(versions), &versions), "v1 policy")) goto done;
    QUIC_REGISTRATION_CONFIG registration_options = {"dotcc-benchmark", QUIC_EXECUTION_PROFILE_LOW_LATENCY};
    if (!check(api->RegistrationOpen(&registration_options, &registration), "RegistrationOpen")) goto done;
    QUIC_SETTINGS settings = {0};
#define SET(Name, Value) settings.IsSet.Name = TRUE; settings.Name = (Value)
    SET(PeerBidiStreamCount, 32); SET(PeerUnidiStreamCount, 0);
    SET(StreamRecvWindowDefault, 1048576); SET(StreamRecvBufferDefault, 1048576); SET(ConnFlowControlWindow, 8388608);
    SET(PacingEnabled, TRUE); SET(EcnEnabled, FALSE); SET(EncryptionOffloadAllowed, FALSE);
    SET(SendBufferingEnabled, FALSE); SET(IdleTimeoutMs, 60000); SET(HandshakeIdleTimeoutMs, 10000); SET(ServerResumptionLevel, QUIC_SERVER_NO_RESUME);
#undef SET
    QUIC_BUFFER alpn = {14, (uint8_t *)"dotcc-bench-v1"};
    if (!check(api->ConfigurationOpen(registration, &alpn, 1, &settings, sizeof(settings), NULL, &configuration), "ConfigurationOpen")) goto done;
    QUIC_CREDENTIAL_CONFIG credentials = {0};
    QUIC_CERTIFICATE_FILE files = {options.key_or_name, options.certificate};
    credentials.Flags = QUIC_CREDENTIAL_FLAG_SET_ALLOWED_CIPHER_SUITES;
    credentials.AllowedCipherSuites = options.cipher == 128 ? QUIC_ALLOWED_CIPHER_SUITE_AES_128_GCM_SHA256 : QUIC_ALLOWED_CIPHER_SUITE_AES_256_GCM_SHA384;
    if (options.server) { credentials.Type = QUIC_CREDENTIAL_TYPE_CERTIFICATE_FILE; credentials.CertificateFile = &files; }
    else {
        credentials.Type = QUIC_CREDENTIAL_TYPE_NONE;
        credentials.Flags |= QUIC_CREDENTIAL_FLAG_CLIENT | QUIC_CREDENTIAL_FLAG_SET_CA_CERTIFICATE_FILE | QUIC_CREDENTIAL_FLAG_USE_TLS_BUILTIN_CERTIFICATE_VALIDATION;
        credentials.CaCertificateFile = options.certificate;
    }
    if (!check(api->ConfigurationLoadCredential(configuration, &credentials), "ConfigurationLoadCredential")) goto done;
    QUIC_ADDR address = {0};
    QUIC_ADDRESS_FAMILY family = strchr(options.ip, ':') ? QUIC_ADDRESS_FAMILY_INET6 : QUIC_ADDRESS_FAMILY_INET;
    QuicAddrSetFamily(&address, family); QuicAddrSetPort(&address, options.port);
    void *destination = family == QUIC_ADDRESS_FAMILY_INET ? (void *)&address.Ipv4.sin_addr : (void *)&address.Ipv6.sin6_addr;
    if (inet_pton(family, options.ip, destination) != 1) { check(QUIC_STATUS_INVALID_PARAMETER, "IP address"); goto done; }
    uint64_t begin;
    if (options.server) {
        if (!check(api->ListenerOpen(registration, listener_callback, NULL, &listener), "ListenerOpen") ||
            !check(api->ListenerStart(listener, &alpn, 1, &address), "ListenerStart")) goto done;
        uint32_t length = sizeof(address);
        if (!check(api->GetParam(listener, QUIC_PARAM_LISTENER_LOCAL_ADDRESS, &length, &address), "listener address")) goto done;
        FILE *ready = fopen(options.ready_file, "w");
        if (!ready) { check(QUIC_STATUS_INTERNAL_ERROR, "ready file"); goto done; }
        fprintf(ready, "%u\n", QuicAddrGetPort(&address)); fclose(ready); begin = now_us();
    } else {
        begin = now_us();
        if (!check(api->ConnectionOpen(registration, connection_callback, NULL, &connection), "ConnectionOpen") ||
            !check(api->SetParam(connection, QUIC_PARAM_CONN_REMOTE_ADDRESS, sizeof(address), &address), "remote address") ||
            !check(api->ConnectionStart(connection, configuration, family, options.key_or_name, options.port), "ConnectionStart")) goto done;
    }
    if (!wait_flag(&connected, 1, 0) || has_failed()) goto done;
    printf("{\"metric\":\"handshake\",\"scope\":\"%s\",\"wall_us\":%llu,\"quic_version\":1,\"group\":23,\"cipher\":%d}\n",
        options.server ? "server_accept_wait" : "client_connect_api", (unsigned long long)(now_us() - begin), (int)handshake.CipherSuite);
    print_resources("idle", -1, sample_resources());
    for (int i = 0; i < options.warmups + options.iterations; i++) if (!transfer_once(&transfers[i])) goto done;
    begin = now_us();
    if (!options.server) api->ConnectionShutdown(connection, QUIC_CONNECTION_SHUTDOWN_FLAG_NONE, 0);
    if (wait_flag(&connection_closed, 1, 0) && !has_failed()) {
        shutdown_us = now_us() - begin;
        shutdown_measured = 1;
    }

done:
    disposal_begin = now_us();
    if (connection && has_failed()) api->ConnectionShutdown(connection, QUIC_CONNECTION_SHUTDOWN_FLAG_NONE, 1);
    for (int i = 0; i < MAX_TRANSFERS; i++) if (transfers[i].stream) api->StreamClose(transfers[i].stream);
    if (connection) api->ConnectionClose(connection);
    if (listener) api->ListenerClose(listener);
    if (configuration) api->ConfigurationClose(configuration);
    if (registration) api->RegistrationClose(registration);
    if (api) MsQuicClose(api);
    disposal_us = now_us() - disposal_begin;
    if (shutdown_measured && !has_failed()) {
        printf("{\"metric\":\"shutdown\",\"scope\":\"%s\",\"wall_us\":%llu}\n",
            options.server ? "server_peer_close_wait" : "client_requested_close", (unsigned long long)shutdown_us);
        printf("{\"metric\":\"owner_disposal\",\"scope\":\"complete_local_owners_after_shutdown\",\"wall_us\":%llu}\n",
            (unsigned long long)disposal_us);
    }
    print_resources("drained", -1, sample_resources());
    printf("{\"passed\":%s,\"clean_close\":%s,\"transport_status\":%u,\"transport_error\":%llu,\"peer_error\":%llu}\n",
        has_failed() ? "false" : "true", has_failed() ? "false" : "true", (unsigned)transport_status,
        (unsigned long long)transport_error, (unsigned long long)peer_error);
    pthread_cond_destroy(&changed); pthread_mutex_destroy(&gate);
    return failed ? 1 : 0;
}
