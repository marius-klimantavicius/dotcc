/* Deterministic upstream portable-crypto and Linux x64 ABI oracle. */
#include <stdint.h>
#include <stddef.h>
#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include <time.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <sys/statvfs.h>
#include "smb2.h"
#include "libsmb2.h"
#include "libsmb2-raw.h"
#include "libsmb2-private.h"
#include "sha.h"
#include "md4.h"
#include "md5.h"
#include "hmac-md5.h"
#include "aes.h"
#include "aes128ccm.h"

/* Internal global API has no public declaration in the pinned signing header. */
void smb3_aes_cmac_128(uint8_t key[16], uint8_t *message, uint64_t length, uint8_t mac[16]);

static void hex(const char *name, const uint8_t *bytes, size_t count)
{
    printf("%s=", name);
    for (size_t i = 0; i < count; i++) printf("%02x", bytes[i]);
    puts("");
}
static void number(const char *name, uint64_t value) { printf("%s=%llu\n", name, (unsigned long long)value); }
static void require_ok(int status) { if (status) { fprintf(stderr, "crypto status %d\n", status); exit(1); } }
#define SIZE(type) number("abi." #type ".size", sizeof(type))
#define FIELD(type, member) number("abi." #type "." #member, offsetof(type, member))

struct completion { uint32_t status; uint64_t cookie; };
static void complete(struct smb2_context *context, int status, void *data, void *opaque)
{
    struct completion *result = opaque;
    (void)context;
    result->status = (uint32_t)status;
    result->cookie = *(uint64_t *)data;
}

int main(void)
{
    uint8_t abc[] = {'a', 'b', 'c'}, digest[64], pattern[257];
    for (unsigned i = 0; i < sizeof(pattern); i++) pattern[i] = (uint8_t)i;
    SHA256Context sha256; SHA512Context sha512;
    require_ok(SHA256Reset(&sha256)); require_ok(SHA256Input(&sha256, abc, 3)); require_ok(SHA256Result(&sha256, digest)); hex("sha256.abc", digest, 32);
    require_ok(SHA512Reset(&sha512)); require_ok(SHA512Input(&sha512, abc, 3)); require_ok(SHA512Result(&sha512, digest)); hex("sha512.abc", digest, 64);
    require_ok(SHA256Reset(&sha256)); require_ok(SHA256Input(&sha256, pattern, 13)); require_ok(SHA256Input(&sha256, pattern + 13, 244)); require_ok(SHA256Result(&sha256, digest)); hex("sha256.pattern257", digest, 32);
    require_ok(SHA512Reset(&sha512)); require_ok(SHA512Input(&sha512, pattern, 13)); require_ok(SHA512Input(&sha512, pattern + 13, 244)); require_ok(SHA512Result(&sha512, digest)); hex("sha512.pattern257", digest, 64);
    MD4_CTX md4; MD4Init(&md4); MD4Update(&md4, abc, 3); MD4Final(digest, &md4); hex("md4.abc", digest, 16);
    struct MD5Context md5; MD5Init(&md5); MD5Update(&md5, abc, 3); MD5Final(digest, &md5); hex("md5.abc", digest, 16);
    uint8_t key[131], hi[] = "Hi There", longtext[] = "Test Using Larger Than Block-Size Key - Hash Key First";
    memset(key, 0x0b, 20);
    smb2_hmac_md5(hi, 8, key, 16, digest); hex("hmac.md5", digest, 16);
    require_ok(hmac(SHA256, hi, 8, key, 20, digest)); hex("hmac.sha256", digest, 32);
    memset(key, 0xaa, sizeof(key));
    require_ok(hmac(SHA256, longtext, sizeof(longtext) - 1, key, sizeof(key), digest)); hex("hmac.sha256.longkey", digest, 32);
    uint8_t block[16], output[16];
    for (unsigned i = 0; i < 16; i++) { key[i] = i; block[i] = (uint8_t)(i * 17); }
    AES128_ECB_encrypt(block, key, output); hex("aes128.ecb", output, 16);
    uint8_t cmac_key[16] = {0x2b,0x7e,0x15,0x16,0x28,0xae,0xd2,0xa6,0xab,0xf7,0x15,0x88,0x09,0xcf,0x4f,0x3c};
    uint8_t cmac_block[16] = {0x6b,0xc1,0xbe,0xe2,0x2e,0x40,0x9f,0x96,0xe9,0x3d,0x7e,0x11,0x73,0x93,0x17,0x2a};
    smb3_aes_cmac_128(cmac_key, cmac_block, 0, output); hex("aes128.cmac.empty", output, 16);
    smb3_aes_cmac_128(cmac_key, cmac_block, 16, output); hex("aes128.cmac.block", output, 16);
    uint8_t nonce[13] = {0,0,0,3,2,1,0,0xa0,0xa1,0xa2,0xa3,0xa4,0xa5}, aad[8], payload[23], ciphertext[23], tag[8];
    for (unsigned i = 0; i < 16; i++) key[i] = (uint8_t)(0xc0 + i);
    for (unsigned i = 0; i < 8; i++) aad[i] = i;
    for (unsigned i = 0; i < 23; i++) payload[i] = (uint8_t)(i + 8);
    aes128ccm_encrypt(key, nonce, 13, aad, 8, payload, 23, tag, 8); hex("aes128.ccm.ciphertext", payload, 23); hex("aes128.ccm.tag", tag, 8);
    memcpy(ciphertext, payload, sizeof(payload));
    number("aes128.ccm.decrypt_ok", aes128ccm_decrypt(key, nonce, 13, aad, 8, payload, 23, tag, 8) == 0); hex("aes128.ccm.plaintext", payload, 23);
    memcpy(payload, ciphertext, sizeof(payload)); tag[0] ^= 1;
    number("aes128.ccm.reject_tag", aes128ccm_decrypt(key, nonce, 13, aad, 8, payload, 23, tag, 8) != 0);
    number("abi.pointer.size", sizeof(void *));
    SIZE(SHA256Context); FIELD(SHA256Context, Message_Block); FIELD(SHA256Context, Computed);
    SIZE(SHA512Context); FIELD(SHA512Context, Message_Block); FIELD(SHA512Context, Computed);
    SIZE(MD4_CTX); SIZE(struct MD5Context);
    SIZE(struct smb2_iovec); FIELD(struct smb2_iovec, len); FIELD(struct smb2_iovec, free);
    SIZE(struct smb2_stat_64); FIELD(struct smb2_stat_64, smb2_size); FIELD(struct smb2_stat_64, smb2_attributes);
    SIZE(struct smb2_statvfs); FIELD(struct smb2_statvfs, f_blocks);
    SIZE(struct smb2_header); FIELD(struct smb2_header, status); FIELD(struct smb2_header, message_id); FIELD(struct smb2_header, session_id); FIELD(struct smb2_header, signature);
    SIZE(struct smb2_pdu); FIELD(struct smb2_pdu, cb); FIELD(struct smb2_pdu, cb_data);
    struct smb2_pdu pdu = {0}; struct completion completed = {0}; uint64_t cookie = UINT64_C(0xfedcba9876543210);
    pdu.cb = complete; pdu.cb_data = &completed;
    pdu.cb(NULL, (int)UINT32_C(0xc0000022), &cookie, pdu.cb_data);
    number("callback.status", completed.status); number("callback.cookie", completed.cookie);
    number("status.access_denied.errno", nterror_to_errno(UINT32_C(0xc0000022)));
    return 0;
}
