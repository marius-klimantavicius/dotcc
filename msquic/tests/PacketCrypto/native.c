/* Test-only native provider oracle. Product code never links this executable. */
#include "quic_platform.h"
#include "quic_tls.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

static void check(int condition, const char* operation)
{
    if (!condition) { fprintf(stderr, "native crypto failed: %s\n", operation); exit(1); }
}

static size_t hex(const char* text, unsigned char* output)
{
    size_t length = strlen(text) / 2;
    for (size_t i = 0; i < length; i++) {
        unsigned int value;
        check(sscanf(text + i * 2, "%2x", &value) == 1, "hex input");
        output[i] = (unsigned char)value;
    }
    return length;
}

static void equal_hex(const unsigned char* actual, const char* expected, const char* name)
{
    unsigned char buffer[1200];
    size_t length = hex(expected, buffer);
    check(memcmp(actual, buffer, length) == 0, name);
}

int main(void)
{
    CxPlatSystemLoad();
    check(CxPlatInitialize() == QUIC_STATUS_SUCCESS, "initialize original native platform");
    unsigned char salt[20], cid[8];
    hex("38762cf7f55934b34d179ae6a4c80cadccbb7f0a", salt);
    hex("8394c8f03e515708", cid);
    QUIC_HKDF_LABELS labels = { "quic key", "quic iv", "quic hp", "quic ku" };
    QUIC_PACKET_KEY *read = NULL, *write = NULL;
    check(QuicPacketKeyCreateInitial(FALSE, &labels, salt, 8, cid, &read, &write) == 0, "initial keys");
    equal_hex(write->Iv, "fa044b2f42a3fd3b46fb255c", "client IV");
    equal_hex(read->Iv, "0ac1493ca1905853b0bba03e", "server IV");
    unsigned char packet[1200] = {0};
    size_t header = hex(INITIAL_HEADER, packet);
    hex(INITIAL_PAYLOAD, packet + header);
    uint64_t number = 2;
    unsigned char nonce[12], mask[16];
    QuicCryptoCombineIvAndPacketNumber(write->Iv, (unsigned char*)&number, nonce);
    check(CxPlatEncrypt(write->PacketKey, nonce, (uint16_t)header, packet,
                       (uint16_t)(sizeof(packet) - header), packet + header) == 0, "initial encrypt");
    check(CxPlatHpComputeMask(write->HeaderKey, 1, packet + header, mask) == 0, "initial HP");
    packet[0] ^= mask[0] & 15;
    for (int i = 1; i <= 4; i++) packet[17 + i] ^= mask[i];
    equal_hex(packet, INITIAL_PACKET, "full initial packet");
    QuicPacketKeyFree(read); QuicPacketKeyFree(write);

    unsigned char material[32], sample[16] = {0};
    for (int i = 0; i < 32; i++) material[i] = (unsigned char)i;
    for (int type = 0; type <= 1; type++) {
        CXPLAT_HP_KEY* hp = NULL;
        check(CxPlatHpKeyCreate((CXPLAT_AEAD_TYPE)type, material, &hp) == 0, "HP create");
        check(CxPlatHpComputeMask(hp, 1, sample, mask) == 0, "HP compute");
        equal_hex(mask, type == 0 ? "c6a13b3787" : "f29000b62a", "AES header vector");
        CxPlatHpKeyFree(hp);
    }
    const char* keys[] = { "5ddd79f7b33f1f4a6dd57c34a8eec42e",
        "3edc6b5b8f7aadbd713732b482b8f979286e1ea3b8f8f99c30c884cfe3349b83" };
    const char* expected[][2] = {
        { "3D49CD6A27AC5CA5F0F51893A755D160", "0D90AEEEA79D7068283FC33561277AD8B641F5D0F2C47A3360DE8BBCB2D1A6E6" },
        { "2775BB8F82B3B5EB667C8CF548C2F06F", "B7BFF374C8928335AA41589D41084B64211876771C459C23B06BA4A2EA89B5AE" }
    };
    uint64_t context = 1752112221;
    for (int k = 0; k < 2; k++) {
        uint32_t length = (uint32_t)hex(keys[k], material);
        for (int o = 0; o < 2; o++) {
            unsigned char output[32];
            check(CxPlatKbKdfDerive(material, length, "test", (unsigned char*)&context,
                                   sizeof(context), (uint32_t)(16 + o * 16), output) == 0, "KBKDF");
            equal_hex(output, expected[k][o], "KBKDF vector");
        }
    }
    CxPlatUninitialize(); CxPlatSystemUnload();
    puts("native packet-crypto: Initial, AES128/256 HP, and four KBKDF controls passed");
    return 0;
}
