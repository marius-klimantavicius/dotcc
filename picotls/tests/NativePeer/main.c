/* Test-only native picotls peer. The managed product does not link this code. */
#include <arpa/inet.h>
#include <errno.h>
#include <signal.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/socket.h>
#include <unistd.h>
#include <openssl/pem.h>
#include "picotls.h"
#include "picotls/openssl.h"

#define MAX_FRAME (8 * 1024 * 1024)
#define ALPN "dotcc-picotls"

static void fail(const char *operation, int error)
{
    fprintf(stderr, "NativePeerError: %s (%d)\n", operation, error);
    exit(1);
}
static void checked(int error, const char *operation) { if (error != 0) fail(operation, error); }
static const char *option(int argc, char **argv, const char *name, const char *fallback)
{
    for (int i = 2; i < argc; i += 2) if (strcmp(argv[i], name) == 0) return argv[i + 1];
    return fallback;
}
static void path(char *output, size_t capacity, const char *directory, const char *identity, const char *suffix)
{
    int length = snprintf(output, capacity, "%s/%s%s", directory, identity, suffix);
    if (length < 0 || (size_t)length >= capacity) fail("path too long", 0);
}
static int hello(ptls_on_client_hello_t *self, ptls_t *tls, ptls_on_client_hello_parameters_t *parameters)
{
    (void)self;
    for (size_t i = 0; i < parameters->negotiated_protocols.count; i++) {
        ptls_iovec_t protocol = parameters->negotiated_protocols.list[i];
        if (protocol.len == sizeof(ALPN) - 1 && memcmp(protocol.base, ALPN, sizeof(ALPN) - 1) == 0) {
            int error = ptls_set_negotiated_protocol(tls, ALPN, sizeof(ALPN) - 1);
            if (error == 0 && parameters->server_name.len != 0)
                error = ptls_set_server_name(tls, (const char *)parameters->server_name.base, parameters->server_name.len);
            return error;
        }
    }
    return PTLS_ALERT_NO_APPLICATION_PROTOCOL;
}
static ptls_on_client_hello_t on_hello = {hello};

struct connection {
    int socket, closed;
    ptls_t *tls;
    ptls_buffer_t output, plaintext;
    ptls_handshake_properties_t properties;
};

static void flush(struct connection *connection)
{
    size_t offset = 0;
    while (offset < connection->output.off) {
        ssize_t sent = send(connection->socket, connection->output.base + offset, connection->output.off - offset, 0);
        if (sent < 0 && errno == EINTR) continue;
        if (sent <= 0) fail("send", errno);
        offset += (size_t)sent;
    }
    connection->output.off = 0;
}
static void pump(struct connection *connection)
{
    unsigned char input[8191];
    ssize_t length;
    do { length = recv(connection->socket, input, sizeof(input), 0); } while (length < 0 && errno == EINTR);
    if (length <= 0) fail("receive before close_notify", length < 0 ? errno : 0);
    size_t offset = 0;
    while (offset < (size_t)length) {
        size_t used = (size_t)length - offset;
        int error;
        if (!ptls_handshake_is_complete(connection->tls)) {
            error = ptls_handshake(connection->tls, &connection->output, input + offset, &used, &connection->properties);
            flush(connection);
            if (error != 0 && error != PTLS_ERROR_IN_PROGRESS) fail("handshake", error);
        } else {
            error = ptls_receive(connection->tls, &connection->plaintext, input + offset, &used);
            if (error == PTLS_ALERT_TO_PEER_ERROR(PTLS_ALERT_CLOSE_NOTIFY)) connection->closed = 1;
            else if (error != 0 && error != PTLS_ERROR_IN_PROGRESS) fail("receive TLS", error);
        }
        if (used == 0) fail("TLS input made no progress", error);
        offset += used;
        if (connection->plaintext.off > MAX_FRAME + 4) fail("application frame limit", 0);
        if (connection->closed && offset != (size_t)length) fail("bytes after close_notify", 0);
    }
}
static void read_plaintext(struct connection *connection, void *output, size_t length)
{
    while (connection->plaintext.off < length) {
        if (connection->closed) fail("truncated application frame", 0);
        pump(connection);
    }
    memcpy(output, connection->plaintext.base, length);
    connection->plaintext.off -= length;
    memmove(connection->plaintext.base, connection->plaintext.base + length, connection->plaintext.off);
}
static unsigned char *read_frame(struct connection *connection, size_t *length)
{
    uint32_t header;
    read_plaintext(connection, &header, sizeof(header));
    *length = ntohl(header);
    if (*length > MAX_FRAME) fail("application frame limit", 0);
    unsigned char *payload = malloc(*length == 0 ? 1 : *length);
    if (payload == NULL) fail("malloc", errno);
    read_plaintext(connection, payload, *length);
    return payload;
}
static void write_frame(struct connection *connection, const unsigned char *payload, size_t length)
{
    uint32_t header = htonl((uint32_t)length);
    checked(ptls_send(connection->tls, &connection->output, &header, sizeof(header)), "send frame header");
    flush(connection);
    for (size_t offset = 0; offset < length;) {
        size_t chunk = length - offset < 4093 ? length - offset : 4093;
        checked(ptls_send(connection->tls, &connection->output, payload + offset, chunk), "send frame body");
        flush(connection);
        offset += chunk;
    }
}

int main(int argc, char **argv)
{
    if (argc < 2 || argc % 2 != 0 || (strcmp(argv[1], "server") != 0 && strcmp(argv[1], "client") != 0))
        fail("use server/client followed by --name value options", 0);
    signal(SIGPIPE, SIG_IGN);
    alarm(30);
    int server = strcmp(argv[1], "server") == 0;
    const char *directory = option(argc, argv, "--credentials", NULL);
    if (directory == NULL) fail("--credentials required", 0);
    const char *identity = option(argc, argv, "--identity", server ? "server-ecdsa" : "");
    const char *target = option(argc, argv, "--target", "localhost");
    const char *ready = option(argc, argv, "--ready", NULL);
    const char *cipher = option(argc, argv, "--cipher", "TLS_AES_256_GCM_SHA384");
    int require_client = strcmp(option(argc, argv, "--require-client-cert", "false"), "true") == 0;
    int update_key = strcmp(option(argc, argv, "--update-key", "false"), "true") == 0;
    int port = atoi(option(argc, argv, "--port", "0"));
    size_t length = (size_t)strtoul(option(argc, argv, "--bytes", "65537"), NULL, 10);
    if (port < 0 || port > 65535 || length > MAX_FRAME) fail("port/bytes out of range", 0);
    ptls_cipher_suite_t *suites[2] = {NULL, NULL};
    if (strcmp(cipher, "TLS_AES_128_GCM_SHA256") == 0) suites[0] = &ptls_openssl_aes128gcmsha256;
    else if (strcmp(cipher, "TLS_AES_256_GCM_SHA384") == 0) suites[0] = &ptls_openssl_aes256gcmsha384;
    else fail("unsupported cipher", 0);
    ptls_key_exchange_algorithm_t *groups[] = {&ptls_openssl_secp256r1, NULL};
    ptls_context_t context = {.random_bytes = ptls_openssl_random_bytes, .get_time = &ptls_get_time,
        .key_exchanges = groups, .cipher_suites = suites, .on_client_hello = &on_hello,
        .require_client_authentication = require_client};
    char file[4096];
    path(file, sizeof(file), directory, "ca", ".pem");
    X509_STORE *store = X509_STORE_new();
    if (store == NULL || X509_STORE_load_locations(store, file, NULL) != 1) fail("load trust root", 0);
    ptls_openssl_verify_certificate_t verifier;
    checked(ptls_openssl_init_verify_certificate(&verifier, store), "initialize certificate verifier");
    X509_STORE_free(store);
    context.verify_certificate = &verifier.super;
    ptls_openssl_sign_certificate_t signer;
    int have_signer = 0;
    if (identity[0] != '\0') {
        path(file, sizeof(file), directory, identity, ".pem");
        checked(ptls_load_certificates(&context, file), "load certificate");
        path(file, sizeof(file), directory, identity, ".key.pem");
        FILE *keyfile = fopen(file, "rb");
        if (keyfile == NULL) fail("open private key", errno);
        EVP_PKEY *key = PEM_read_PrivateKey(keyfile, NULL, NULL, NULL);
        fclose(keyfile);
        if (key == NULL) fail("parse private key", 0);
        checked(ptls_openssl_init_sign_certificate(&signer, key), "initialize signer");
        EVP_PKEY_free(key);
        context.sign_certificate = &signer.super;
        have_signer = 1;
    }
    int fd = socket(AF_INET, SOCK_STREAM, 0);
    if (fd < 0) fail("socket", errno);
    struct sockaddr_in address = {.sin_family = AF_INET, .sin_port = htons((uint16_t)port), .sin_addr.s_addr = htonl(INADDR_LOOPBACK)};
    if (server) {
        if (!have_signer) fail("server requires identity", 0);
        if (bind(fd, (struct sockaddr *)&address, sizeof(address)) != 0 || listen(fd, 1) != 0) fail("listen", errno);
        socklen_t size = sizeof(address);
        if (getsockname(fd, (struct sockaddr *)&address, &size) != 0) fail("getsockname", errno);
        if (ready == NULL) fail("server requires --ready", 0);
        char temporary[4096];
        if (snprintf(temporary, sizeof(temporary), "%s.tmp", ready) >= (int)sizeof(temporary)) fail("ready path too long", 0);
        FILE *output = fopen(temporary, "w");
        if (output == NULL) fail("ready file", errno);
        fprintf(output, "{\"Port\":%u}\n", ntohs(address.sin_port));
        if (fclose(output) != 0 || rename(temporary, ready) != 0) fail("publish ready", errno);
        int accepted = accept(fd, NULL, NULL);
        close(fd);
        if (accepted < 0) fail("accept", errno);
        fd = accepted;
    } else if (connect(fd, (struct sockaddr *)&address, sizeof(address)) != 0) fail("connect", errno);
    struct connection connection = {.socket = fd};
    ptls_buffer_init(&connection.output, "", 0);
    ptls_buffer_init(&connection.plaintext, "", 0);
    connection.tls = ptls_new(&context, server);
    if (connection.tls == NULL) fail("ptls_new", 0);
    ptls_iovec_t protocol = ptls_iovec_init(ALPN, sizeof(ALPN) - 1);
    if (!server) {
        checked(ptls_set_server_name(connection.tls, target, 0), "server name");
        connection.properties.client.negotiated_protocols.list = &protocol;
        connection.properties.client.negotiated_protocols.count = 1;
        int result = ptls_handshake(connection.tls, &connection.output, NULL, NULL, &connection.properties);
        if (result != PTLS_ERROR_IN_PROGRESS) fail("start handshake", result);
        flush(&connection);
    }
    while (!ptls_handshake_is_complete(connection.tls)) pump(&connection);
    const char *negotiated = ptls_get_negotiated_protocol(connection.tls);
    if (ptls_get_protocol_version(connection.tls) != PTLS_PROTOCOL_VERSION_TLS13 || negotiated == NULL || strcmp(negotiated, ALPN) != 0)
        fail("TLS1.3/ALPN required", 0);
    if (update_key) checked(ptls_update_key(connection.tls, 1), "key update");
    unsigned char *payload;
    if (server) {
        payload = read_frame(&connection, &length);
        write_frame(&connection, payload, length);
    } else {
        payload = malloc(length == 0 ? 1 : length);
        if (payload == NULL) fail("malloc payload", errno);
        for (size_t i = 0; i < length; i++) payload[i] = (unsigned char)(i * 31 + 7);
        write_frame(&connection, payload, length);
        size_t echoed_length;
        unsigned char *echoed = read_frame(&connection, &echoed_length);
        if (echoed_length != length || memcmp(payload, echoed, length) != 0) fail("echo mismatch", 0);
        free(echoed);
    }
    checked(ptls_send_alert(connection.tls, &connection.output, PTLS_ALERT_LEVEL_WARNING, PTLS_ALERT_CLOSE_NOTIFY), "send close_notify");
    flush(&connection);
    while (!connection.closed) pump(&connection);
    if (connection.plaintext.off != 0) fail("unexpected application data", 0);
    unsigned char digest[EVP_MAX_MD_SIZE];
    unsigned int digest_size;
    if (EVP_Digest(payload, length, digest, &digest_size, EVP_sha256(), NULL) != 1 || digest_size != 32) fail("payload hash", 0);
    char hash[65];
    for (size_t i = 0; i < 32; i++) snprintf(hash + i * 2, 3, "%02X", digest[i]);
    printf("{\"Role\":\"%s\",\"Protocol\":\"Tls13\",\"Cipher\":\"%s\",\"Alpn\":\"" ALPN "\",\"Bytes\":%zu,\"Sha256\":\"%s\"}\n",
           server ? "server" : "client", ptls_get_cipher(connection.tls)->name, length, hash);
    free(payload);
    ptls_free(connection.tls);
    ptls_buffer_dispose(&connection.output);
    ptls_buffer_dispose(&connection.plaintext);
    close(fd);
    for (size_t i = 0; i < context.certificates.count; i++) free(context.certificates.list[i].base);
    free(context.certificates.list);
    if (have_signer) ptls_openssl_dispose_sign_certificate(&signer);
    ptls_openssl_dispose_verify_certificate(&verifier);
    return 0;
}
