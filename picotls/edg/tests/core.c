/* Exercises the pinned, unchanged picotls core. No cryptographic substitutes. */
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "picotls.h"
#include "picotls/pembase64.h"

#define CHECK(test) do { if (!(test)) { fprintf(stderr, "check failed at line %d: %s\n", __LINE__, #test); return 1; } } while (0)

static int buffers(void)
{
    uint8_t small[8], payload[4097], asn1[] = {0, 0, 0x80, 0x11};
    const uint8_t expected[] = {2, 3, 0, 0x80, 0x11};
    ptls_buffer_t buf;
    ptls_buffer_init(&buf, small, sizeof(small));
    for (size_t i = 0; i != sizeof(payload); ++i) payload[i] = (uint8_t)(i * 31);
    CHECK(ptls_buffer__do_pushv(&buf, payload, 5) == 0);
    CHECK(!buf.is_allocated && buf.base == small && buf.off == 5);
    CHECK(ptls_buffer__do_pushv(&buf, payload + 5, sizeof(payload) - 5) == 0);
    CHECK(buf.is_allocated && buf.off == sizeof(payload));
    CHECK(memcmp(buf.base, payload, sizeof(payload)) == 0);
    CHECK(ptls_buffer_reserve_aligned(&buf, 8192, 6) == 0);
    CHECK(((uintptr_t)buf.base & 63) == 0);
    CHECK(memcmp(buf.base, payload, sizeof(payload)) == 0);
    ptls_buffer_dispose(&buf);
    ptls_buffer_init(&buf, small, sizeof(small));
    CHECK(ptls_buffer_push_asn1_ubigint(&buf, asn1, sizeof(asn1)) == 0);
    CHECK(buf.off == sizeof(expected) && memcmp(buf.base, expected, sizeof(expected)) == 0);
    ptls_buffer_dispose(&buf);
    puts("buffers: stack, growth, 64-byte alignment, preservation, ASN.1 passed");
    return 0;
}

static int integers(void)
{
    const uint64_t values[] = {0, 1, 63, 64, 15293, 16383, 16384, 494878333, 1073741823,
                              1073741824, UINT64_C(151288809941952652), UINT64_C(4611686018427387903)};
    uint8_t data[8];
    for (size_t i = 0; i != sizeof(values) / sizeof(values[0]); ++i) {
        uint8_t *end = ptls_encode_quicint(data, values[i]);
        size_t len = (size_t)(end - data);
        const uint8_t *p = data;
        CHECK(len == (values[i] < 64 ? 1 : values[i] < 16384 ? 2 : values[i] < UINT64_C(1073741824) ? 4 : 8));
        CHECK(ptls_decode_quicint(&p, end) == values[i] && p == end);
        for (size_t n = 0; n != len; ++n) {
            p = data;
            CHECK(ptls_decode_quicint(&p, data + n) == UINT64_MAX);
        }
    }
    const uint8_t wire[] = {0x12, 0x34, 0x56, 0x78, 0x9a, 0xbc, 0xde, 0xf0};
    const uint8_t *p = wire;
    uint64_t v64;
    uint32_t v32;
    uint16_t v16;
    uint8_t v8;
    CHECK(ptls_decode64(&v64, &p, wire + 8) == 0 && v64 == UINT64_C(0x123456789abcdef0));
    p = wire;
    CHECK(ptls_decode8(&v8, &p, wire + 8) == 0 && v8 == 0x12);
    CHECK(ptls_decode16(&v16, &p, wire + 8) == 0 && v16 == 0x3456);
    CHECK(ptls_decode24(&v32, &p, wire + 8) == 0 && v32 == 0x789abc);
    CHECK(ptls_decode32(&v32, &p, wire + 8) == PTLS_ALERT_DECODE_ERROR && p == wire + 6);
    p = wire;
    CHECK(ptls_decode32(&v32, &p, wire + 8) == 0 && v32 == UINT32_C(0x12345678));
    puts("integers: 12 QUIC boundaries, every truncation, network byte order passed");
    return 0;
}

static int base64(void)
{
    const char *plain[] = {"", "f", "fo", "foo", "foob", "fooba", "foobar"};
    const char *encoded[] = {"", "Zg==", "Zm8=", "Zm9v", "Zm9vYg==", "Zm9vYmE=", "Zm9vYmFy"};
    char text[32];
    uint8_t small[2];
    ptls_buffer_t buf;
    ptls_base64_decode_state_t state;
    for (size_t i = 0; i != 7; ++i) {
        CHECK(ptls_base64_howlong(strlen(plain[i])) == strlen(encoded[i]));
        CHECK(ptls_base64_encode((const uint8_t *)plain[i], strlen(plain[i]), text) == (int)strlen(encoded[i]) + 1);
        CHECK(strcmp(text, encoded[i]) == 0);
        ptls_buffer_init(&buf, small, sizeof(small));
        ptls_base64_decode_init(&state);
        CHECK(ptls_base64_decode(encoded[i], &state, &buf) == 0);
        CHECK(buf.off == strlen(plain[i]) && memcmp(buf.base, plain[i], buf.off) == 0);
        ptls_buffer_dispose(&buf);
    }
    ptls_buffer_init(&buf, small, sizeof(small));
    ptls_base64_decode_init(&state);
    CHECK(ptls_base64_decode(" Zm", &state, &buf) == 0);
    CHECK(ptls_base64_decode("9vYmFy\r\n", &state, &buf) == 0);
    CHECK(buf.off == 6 && memcmp(buf.base, "foobar", 6) == 0);
    ptls_buffer_dispose(&buf);
    ptls_buffer_init(&buf, small, sizeof(small));
    ptls_base64_decode_init(&state);
    CHECK(ptls_base64_decode("Zm!v", &state, &buf) == PTLS_ERROR_INCORRECT_BASE64);
    CHECK(state.status == PTLS_BASE64_DECODE_FAILED);
    ptls_buffer_dispose(&buf);
    puts("base64: RFC 4648 vectors, streaming, whitespace, malformed input passed");
    return 0;
}

static int utilities(void)
{
    uint8_t bytes[] = {0, 1, 0x7f, 0x80, 0xff};
    uint8_t zero[sizeof(bytes)] = {0};
    uint8_t mapped[16], ipv4[] = {192, 0, 2, 1};
    const uint8_t expected[16] = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 0, 2, 1};
    char hex[sizeof(bytes) * 2 + 1], json[64];
    CHECK(ptls_hexdump(hex, bytes, sizeof(bytes)) == hex);
    CHECK(strcmp(hex, "00017f80ff") == 0);
    CHECK(ptls_mem_equal(bytes, bytes, sizeof(bytes)) == 1);
    CHECK(ptls_mem_equal(bytes, zero, sizeof(bytes)) == 0);
    ptls_clear_memory(bytes, sizeof(bytes));
    CHECK(ptls_mem_equal(bytes, zero, sizeof(bytes)) == 1);
    ptls_jsonescape(json, "a\"\\\n", 4);
    CHECK(strcmp(json, "a\\\"\\\\\\n") == 0);
    ptls_build_v4_mapped_v6_address(mapped, ipv4);
    CHECK(memcmp(mapped, expected, sizeof(mapped)) == 0);
    CHECK(ptls_server_name_is_ipaddr("127.0.0.1") == 1);
    CHECK(ptls_server_name_is_ipaddr("2001:db8::1") == 1);
    CHECK(ptls_server_name_is_ipaddr("example.com") == 0);
    CHECK(ptls_server_name_is_ipaddr("300.0.0.1") == 0);
    puts("utilities: memory callbacks, escaping, hex, IPv4/IPv6 passed");
    return 0;
}

static int pem(void)
{
    const char text[] = "-----BEGIN TEST-----\nZm9vYmFy\n-----END TEST-----\n"
                        "-----BEGIN TEST-----\nZg==\n-----END TEST-----\n";
    const char *path = "picotls-core-fixture.pem";
    ptls_iovec_t list[3];
    size_t count = 0;
    FILE *f = fopen(path, "wb");
    CHECK(f != NULL);
    CHECK(fwrite(text, 1, sizeof(text) - 1, f) == sizeof(text) - 1);
    CHECK(fclose(f) == 0);
    CHECK(ptls_load_pem_objects(path, "TEST", list, 3, &count) == 0);
    CHECK(count == 2 && list[0].len == 6 && list[1].len == 1);
    CHECK(memcmp(list[0].base, "foobar", 6) == 0 && list[1].base[0] == 'f');
    for (size_t i = 0; i != count; ++i) free(list[i].base);
    CHECK(remove(path) == 0);
    puts("PEM: real filesystem, two objects, allocation ownership passed");
    return 0;
}

int main(void)
{
    if (buffers() || integers() || base64() || utilities() || pem()) return 1;
    puts("picotls unchanged core differential fixture passed");
    return 0;
}
