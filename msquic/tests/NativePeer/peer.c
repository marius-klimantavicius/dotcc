/* Separate native oracle. This file never enters the translated product. */
#include <msquic.h>
#include <stdatomic.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>

#define PAYLOAD_SIZE 65537
static const QUIC_API_TABLE *api;
static HQUIC server_configuration;
static atomic_int failure;
typedef struct Peer {
    int server;
    HQUIC connection;
    HQUIC stream;
    uint64_t received;
    atomic_int connected, finished, closed;
    QUIC_HANDSHAKE_INFO handshake;
    uint32_t version;
} Peer;
static Peer server_peer = {.server = 1}, client_peer;

static void check(QUIC_STATUS status, const char *operation) {
    if (QUIC_FAILED(status)) {
        fprintf(stderr, "%s failed: 0x%x\n", operation, status);
        atomic_store(&failure, 1);
    }
}

static unsigned char pattern(uint64_t offset, int sender) {
    return (unsigned char)(offset * 31 + 17 + sender * 29);
}

static void send_payload(Peer *peer) {
    QUIC_BUFFER *buffer = malloc(sizeof(*buffer) + PAYLOAD_SIZE);
    if (!buffer) { atomic_store(&failure, 1); return; }
    buffer->Length = PAYLOAD_SIZE;
    buffer->Buffer = (uint8_t *)(buffer + 1);
    for (uint32_t i = 0; i < PAYLOAD_SIZE; i++) buffer->Buffer[i] = pattern(i, peer->server);
    QUIC_STATUS status = api->StreamSend(peer->stream, buffer, 1, QUIC_SEND_FLAG_FIN, buffer);
    if (QUIC_FAILED(status)) { free(buffer); check(status, "StreamSend"); }
}

static QUIC_STATUS QUIC_API stream_callback(HQUIC stream, void *context, QUIC_STREAM_EVENT *event) {
    (void)stream;
    Peer *peer = context;
    switch (event->Type) {
    case QUIC_STREAM_EVENT_RECEIVE:
        if (event->RECEIVE.AbsoluteOffset != peer->received) atomic_store(&failure, 1);
        for (uint32_t b = 0; b < event->RECEIVE.BufferCount; b++)
            for (uint32_t i = 0; i < event->RECEIVE.Buffers[b].Length; i++) {
                if (peer->received >= PAYLOAD_SIZE || event->RECEIVE.Buffers[b].Buffer[i] != pattern(peer->received, !peer->server))
                    atomic_store(&failure, 1);
                peer->received++;
            }
        break;
    case QUIC_STREAM_EVENT_PEER_SEND_SHUTDOWN:
        if (peer->received != PAYLOAD_SIZE) atomic_store(&failure, 1);
        if (peer->server) send_payload(peer);
        atomic_store(&peer->finished, 1);
        break;
    case QUIC_STREAM_EVENT_SEND_COMPLETE:
        free(event->SEND_COMPLETE.ClientContext);
        if (event->SEND_COMPLETE.Canceled) atomic_store(&failure, 1);
        break;
    case QUIC_STREAM_EVENT_PEER_SEND_ABORTED:
    case QUIC_STREAM_EVENT_PEER_RECEIVE_ABORTED:
        atomic_store(&failure, 1);
        break;
    default: break;
    }
    return QUIC_STATUS_SUCCESS;
}

static QUIC_STATUS QUIC_API connection_callback(HQUIC connection, void *context, QUIC_CONNECTION_EVENT *event) {
    Peer *peer = context;
    switch (event->Type) {
    case QUIC_CONNECTION_EVENT_CONNECTED: {
        uint32_t length = sizeof(peer->handshake);
        check(api->GetParam(connection, QUIC_PARAM_TLS_HANDSHAKE_INFO, &length, &peer->handshake), "handshake info");
        length = sizeof(peer->version);
        check(api->GetParam(connection, QUIC_PARAM_CONN_QUIC_VERSION, &length, &peer->version), "QUIC version");
        if (event->CONNECTED.NegotiatedAlpnLength != 11 || memcmp(event->CONNECTED.NegotiatedAlpn, "dotcc-probe", 11))
            atomic_store(&failure, 1);
        atomic_store(&peer->connected, 1);
        if (!peer->server) {
            check(api->StreamOpen(connection, QUIC_STREAM_OPEN_FLAG_NONE, stream_callback, peer, &peer->stream), "StreamOpen");
            if (peer->stream) {
                check(api->StreamStart(peer->stream, QUIC_STREAM_START_FLAG_IMMEDIATE), "StreamStart");
                send_payload(peer);
            }
        }
        break;
    }
    case QUIC_CONNECTION_EVENT_PEER_STREAM_STARTED:
        if (!peer->server || peer->stream) { atomic_store(&failure, 1); break; }
        peer->stream = event->PEER_STREAM_STARTED.Stream;
        api->SetCallbackHandler(peer->stream, (void *)stream_callback, peer);
        break;
    case QUIC_CONNECTION_EVENT_SHUTDOWN_INITIATED_BY_TRANSPORT:
        check(event->SHUTDOWN_INITIATED_BY_TRANSPORT.Status, "transport shutdown");
        break;
    case QUIC_CONNECTION_EVENT_SHUTDOWN_COMPLETE:
        atomic_store(&peer->closed, 1);
        break;
    default: break;
    }
    return QUIC_STATUS_SUCCESS;
}

static QUIC_STATUS QUIC_API listener_callback(HQUIC listener, void *context, QUIC_LISTENER_EVENT *event) {
    (void)listener; (void)context;
    if (event->Type == QUIC_LISTENER_EVENT_NEW_CONNECTION) {
        if (server_peer.connection) return QUIC_STATUS_CONNECTION_REFUSED;
        server_peer.connection = event->NEW_CONNECTION.Connection;
        api->SetCallbackHandler(server_peer.connection, (void *)connection_callback, &server_peer);
        return api->ConnectionSetConfiguration(server_peer.connection, server_configuration);
    }
    return QUIC_STATUS_SUCCESS;
}

static int wait_for(atomic_int *flag) {
    struct timespec pause = {0, 1000000};
    for (int i = 0; i < 15000; i++) {
        if (atomic_load(flag)) return 1;
        if (atomic_load(&failure)) return 0;
        nanosleep(&pause, NULL);
    }
    atomic_store(&failure, 1);
    return 0;
}

int main(int argc, char **argv) {
    if (!((argc >= 5 && argc <= 7) || argc == 10)) {
        fprintf(stderr, "peer certificate key cipher128|256 ipv4|ipv6 [trust-file [server-name [client|server port ready-file]]]\n");
        return 2;
    }
    const int external = argc == 10;
    const int run_client = !external || !strcmp(argv[7], "client");
    const int run_server = !external || !strcmp(argv[7], "server");
    if (!run_client && !run_server) return 2;
    if (strcmp(argv[3], "128") && strcmp(argv[3], "256")) return 2;
    if (strcmp(argv[4], "ipv4") && strcmp(argv[4], "ipv6")) return 2;
    char *port_end = NULL;
    long port = external ? strtol(argv[8], &port_end, 10) : 0;
    if (external && (!*argv[8] || *port_end || port < 0 || port > 65535 || (run_client && !port))) return 2;
    check(MsQuicOpen2(&api), "MsQuicOpen2");
    if (!api) return 1;
    HQUIC registration = NULL, listener = NULL, client_configuration = NULL;
    QUIC_REGISTRATION_CONFIG registration_config = {"dotcc-native-oracle", QUIC_EXECUTION_PROFILE_LOW_LATENCY};
    check(api->RegistrationOpen(&registration_config, &registration), "RegistrationOpen");
    QUIC_SETTINGS settings = {0};
    settings.IsSet.PeerBidiStreamCount = TRUE; settings.PeerBidiStreamCount = 1;
    settings.IsSet.IdleTimeoutMs = TRUE; settings.IdleTimeoutMs = 10000;
    QUIC_BUFFER alpn = {11, (uint8_t *)"dotcc-probe"};
    if (run_server) check(api->ConfigurationOpen(registration, &alpn, 1, &settings, sizeof(settings), NULL, &server_configuration), "server ConfigurationOpen");
    if (run_client) check(api->ConfigurationOpen(registration, &alpn, 1, &settings, sizeof(settings), NULL, &client_configuration), "client ConfigurationOpen");
    QUIC_CERTIFICATE_FILE files = {argv[2], argv[1]};
    QUIC_CREDENTIAL_CONFIG credential = {0};
    credential.Type = QUIC_CREDENTIAL_TYPE_CERTIFICATE_FILE;
    credential.CertificateFile = &files;
    credential.Flags = QUIC_CREDENTIAL_FLAG_SET_ALLOWED_CIPHER_SUITES;
    credential.AllowedCipherSuites = !strcmp(argv[3], "128") ? QUIC_ALLOWED_CIPHER_SUITE_AES_128_GCM_SHA256 : QUIC_ALLOWED_CIPHER_SUITE_AES_256_GCM_SHA384;
    if (run_server) check(api->ConfigurationLoadCredential(server_configuration, &credential), "server credential");
    credential.Type = QUIC_CREDENTIAL_TYPE_NONE;
    credential.CertificateFile = NULL;
    credential.Flags |= QUIC_CREDENTIAL_FLAG_CLIENT | QUIC_CREDENTIAL_FLAG_SET_CA_CERTIFICATE_FILE |
        QUIC_CREDENTIAL_FLAG_USE_TLS_BUILTIN_CERTIFICATE_VALIDATION;
    credential.CaCertificateFile = argc > 5 ? argv[5] : argv[1];
    if (run_client) check(api->ConfigurationLoadCredential(client_configuration, &credential), "client credential");
    QUIC_ADDR address = {0};
    const QUIC_ADDRESS_FAMILY family = !strcmp(argv[4], "ipv4") ? QUIC_ADDRESS_FAMILY_INET : QUIC_ADDRESS_FAMILY_INET6;
    QuicAddrSetFamily(&address, family);
    QuicAddrSetToLoopback(&address);
    QuicAddrSetPort(&address, (uint16_t)port);
    if (run_server) {
        check(api->ListenerOpen(registration, listener_callback, NULL, &listener), "ListenerOpen");
        check(api->ListenerStart(listener, &alpn, 1, &address), "ListenerStart");
        uint32_t address_size = sizeof(address);
        check(api->GetParam(listener, QUIC_PARAM_LISTENER_LOCAL_ADDRESS, &address_size, &address), "listener address");
        if (external && !atomic_load(&failure)) {
            FILE *ready = fopen(argv[9], "w");
            if (!ready) atomic_store(&failure, 1);
            else { fprintf(ready, "%u\n", QuicAddrGetPort(&address)); fclose(ready); }
        }
    }
    if (run_client) {
        check(api->ConnectionOpen(registration, connection_callback, &client_peer, &client_peer.connection), "ConnectionOpen");
        check(api->SetParam(client_peer.connection, QUIC_PARAM_CONN_REMOTE_ADDRESS, sizeof(address), &address), "remote address");
        check(api->ConnectionStart(client_peer.connection, client_configuration, family,
            argc > 6 ? argv[6] : "localhost", QuicAddrGetPort(&address)), "ConnectionStart");
        wait_for(&client_peer.finished);
        api->ConnectionShutdown(client_peer.connection, QUIC_CONNECTION_SHUTDOWN_FLAG_NONE, 0);
        wait_for(&client_peer.closed);
    }
    if (run_server) wait_for(&server_peer.closed);
    if (client_peer.stream) api->StreamClose(client_peer.stream);
    if (server_peer.stream) api->StreamClose(server_peer.stream);
    if (client_peer.connection) api->ConnectionClose(client_peer.connection);
    if (server_peer.connection) api->ConnectionClose(server_peer.connection);
    if (listener) api->ListenerClose(listener);
    if (client_configuration) api->ConfigurationClose(client_configuration);
    if (server_configuration) api->ConfigurationClose(server_configuration);
    api->RegistrationClose(registration);
    MsQuicClose(api);
    int cipher = !strcmp(argv[3], "128") ? QUIC_CIPHER_SUITE_TLS_AES_128_GCM_SHA256 : QUIC_CIPHER_SUITE_TLS_AES_256_GCM_SHA384;
    Peer *peers[2] = {&client_peer, &server_peer};
    for (int i = 0; i < 2; i++) {
        if ((i == 0 && !run_client) || (i == 1 && !run_server)) continue;
        Peer *peer = peers[i];
        if (!atomic_load(&peer->connected) || !atomic_load(&peer->finished) ||
            peer->received != PAYLOAD_SIZE || peer->handshake.CipherSuite != cipher ||
            peer->handshake.TlsGroup != QUIC_TLS_GROUP_SECP256R1 || peer->version != 1)
            atomic_store(&failure, 1);
    }
    Peer *report = run_client ? &client_peer : &server_peer;
    printf("{\"passed\":%s,\"family\":\"%s\",\"cipher\":%d,\"group\":%d,\"quic_version\":%u,\"client_bytes\":%llu,\"server_bytes\":%llu,\"certificate_validation\":%s}\n",
        atomic_load(&failure) ? "false" : "true", argv[4], report->handshake.CipherSuite,
        report->handshake.TlsGroup, report->version,
        (unsigned long long)client_peer.received, (unsigned long long)server_peer.received,
        run_client ? "true" : "false");
    return atomic_load(&failure) ? 1 : 0;
}
