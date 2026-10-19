/* Actual same-pin HACL code and endian helpers. Native hashlib supplied
 * fixed empty/abc/200-byte vectors; no hashing implementation lives here. */
#include <assert.h>
#include <stdio.h>
#include <string.h>
#include "Hacl_Hash_SHA2.h"

#if __BYTE_ORDER__ != __ORDER_LITTLE_ENDIAN__
#error This campaign control requires the selected little-endian target
#endif

static void check_hex(const uint8_t *digest, const char *hex, int length) {
    const char *digits="0123456789abcdef";
    for(int i=0;i<length;++i) {
        assert(digits[digest[i]>>4]==hex[2*i]);
        assert(digits[digest[i]&15]==hex[2*i+1]);
    }
}

static void check_224(void) {
    uint8_t input[200], digest[28], repeated[28];
    memset(input,'a',200);
    Hacl_Hash_SHA2_hash_224(digest,input,0);
    check_hex(digest,"d14a028c2a3a2bc9476102bb288234c415a2b01f828ea62ac5b3e42f",28);
    uint8_t abc[3]={'a','b','c'};
    Hacl_Hash_SHA2_hash_224(digest,abc,3);
    check_hex(digest,"23097d223405d8228642a477bda255b32aadbce4bda0b3f7e36c9da7",28);
    Hacl_Hash_SHA2_hash_224(digest,input,200);
    check_hex(digest,"2559984fd15e055f0d84c346483508242f02653ab7956401e551511c",28);
    Hacl_Streaming_MD_state_32 *stream=Hacl_Hash_SHA2_malloc_224();
    assert(stream!=NULL);
    assert(Hacl_Hash_SHA2_update_224(stream,input,1)==0);
    Hacl_Streaming_MD_state_32 *clone=Hacl_Hash_SHA2_copy_256(stream);
    assert(clone!=NULL);
    Hacl_Hash_SHA2_digest_224(stream,repeated);
    assert(Hacl_Hash_SHA2_update_224(stream,input+1,199)==0);
    Hacl_Hash_SHA2_digest_224(stream,repeated);
    assert(memcmp(digest,repeated,28)==0);
    assert(Hacl_Hash_SHA2_update_224(clone,input+1,63)==0);
    assert(Hacl_Hash_SHA2_update_224(clone,input+64,136)==0);
    Hacl_Hash_SHA2_digest_224(clone,repeated);
    assert(memcmp(digest,repeated,28)==0);
    Hacl_Hash_SHA2_reset_224(stream);
    assert(Hacl_Hash_SHA2_update_224(stream,abc,3)==0);
    Hacl_Hash_SHA2_digest_224(stream,repeated);
    check_hex(repeated,"23097d223405d8228642a477bda255b32aadbce4bda0b3f7e36c9da7",28);
    Hacl_Hash_SHA2_free_224(clone);
    Hacl_Hash_SHA2_free_224(stream);
    puts("sha224 vectors-stream-copy-reset-ok");
}

static void check_256(void) {
    uint8_t input[200], digest[32], repeated[32];
    memset(input,'a',200);
    Hacl_Hash_SHA2_hash_256(digest,input,0);
    check_hex(digest,"e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",32);
    uint8_t abc[3]={'a','b','c'};
    Hacl_Hash_SHA2_hash_256(digest,abc,3);
    check_hex(digest,"ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad",32);
    Hacl_Hash_SHA2_hash_256(digest,input,200);
    check_hex(digest,"c2a908d98f5df987ade41b5fce213067efbcc21ef2240212a41e54b5e7c28ae5",32);
    Hacl_Streaming_MD_state_32 *stream=Hacl_Hash_SHA2_malloc_256();
    assert(stream!=NULL);
    assert(Hacl_Hash_SHA2_update_256(stream,input,1)==0);
    Hacl_Streaming_MD_state_32 *clone=Hacl_Hash_SHA2_copy_256(stream);
    assert(clone!=NULL);
    Hacl_Hash_SHA2_digest_256(stream,repeated);
    assert(Hacl_Hash_SHA2_update_256(stream,input+1,199)==0);
    Hacl_Hash_SHA2_digest_256(stream,repeated);
    assert(memcmp(digest,repeated,32)==0);
    assert(Hacl_Hash_SHA2_update_256(clone,input+1,63)==0);
    assert(Hacl_Hash_SHA2_update_256(clone,input+64,136)==0);
    Hacl_Hash_SHA2_digest_256(clone,repeated);
    assert(memcmp(digest,repeated,32)==0);
    Hacl_Hash_SHA2_reset_256(stream);
    assert(Hacl_Hash_SHA2_update_256(stream,abc,3)==0);
    Hacl_Hash_SHA2_digest_256(stream,repeated);
    check_hex(repeated,"ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad",32);
    Hacl_Hash_SHA2_free_256(clone);
    Hacl_Hash_SHA2_free_256(stream);
    puts("sha256 vectors-stream-copy-reset-ok");
}

static void check_384(void) {
    uint8_t input[200], digest[48], repeated[48];
    memset(input,'a',200);
    Hacl_Hash_SHA2_hash_384(digest,input,0);
    check_hex(digest,"38b060a751ac96384cd9327eb1b1e36a21fdb71114be07434c0cc7bf63f6e1da274edebfe76f65fbd51ad2f14898b95b",48);
    uint8_t abc[3]={'a','b','c'};
    Hacl_Hash_SHA2_hash_384(digest,abc,3);
    check_hex(digest,"cb00753f45a35e8bb5a03d699ac65007272c32ab0eded1631a8b605a43ff5bed8086072ba1e7cc2358baeca134c825a7",48);
    Hacl_Hash_SHA2_hash_384(digest,input,200);
    check_hex(digest,"0691b6e978614b67d60557b2a2cddd53406508522efa21c624dbbfa8ab6e726d5c586b489c7c09f24109a64c10211d48",48);
    Hacl_Streaming_MD_state_64 *stream=Hacl_Hash_SHA2_malloc_384();
    assert(stream!=NULL);
    assert(Hacl_Hash_SHA2_update_384(stream,input,1)==0);
    Hacl_Streaming_MD_state_64 *clone=Hacl_Hash_SHA2_copy_512(stream);
    assert(clone!=NULL);
    Hacl_Hash_SHA2_digest_384(stream,repeated);
    assert(Hacl_Hash_SHA2_update_384(stream,input+1,199)==0);
    Hacl_Hash_SHA2_digest_384(stream,repeated);
    assert(memcmp(digest,repeated,48)==0);
    assert(Hacl_Hash_SHA2_update_384(clone,input+1,63)==0);
    assert(Hacl_Hash_SHA2_update_384(clone,input+64,136)==0);
    Hacl_Hash_SHA2_digest_384(clone,repeated);
    assert(memcmp(digest,repeated,48)==0);
    Hacl_Hash_SHA2_reset_384(stream);
    assert(Hacl_Hash_SHA2_update_384(stream,abc,3)==0);
    Hacl_Hash_SHA2_digest_384(stream,repeated);
    check_hex(repeated,"cb00753f45a35e8bb5a03d699ac65007272c32ab0eded1631a8b605a43ff5bed8086072ba1e7cc2358baeca134c825a7",48);
    Hacl_Hash_SHA2_free_384(clone);
    Hacl_Hash_SHA2_free_384(stream);
    puts("sha384 vectors-stream-copy-reset-ok");
}

static void check_512(void) {
    uint8_t input[200], digest[64], repeated[64];
    memset(input,'a',200);
    Hacl_Hash_SHA2_hash_512(digest,input,0);
    check_hex(digest,"cf83e1357eefb8bdf1542850d66d8007d620e4050b5715dc83f4a921d36ce9ce47d0d13c5d85f2b0ff8318d2877eec2f63b931bd47417a81a538327af927da3e",64);
    uint8_t abc[3]={'a','b','c'};
    Hacl_Hash_SHA2_hash_512(digest,abc,3);
    check_hex(digest,"ddaf35a193617abacc417349ae20413112e6fa4e89a97ea20a9eeee64b55d39a2192992a274fc1a836ba3c23a3feebbd454d4423643ce80e2a9ac94fa54ca49f",64);
    Hacl_Hash_SHA2_hash_512(digest,input,200);
    check_hex(digest,"4b11459c33f52a22ee8236782714c150a3b2c60994e9acee17fe68947a3e6789f31e7668394592da7bef827cddca88c4e6f86e4df7ed1ae6cba71f3e98faee9f",64);
    Hacl_Streaming_MD_state_64 *stream=Hacl_Hash_SHA2_malloc_512();
    assert(stream!=NULL);
    assert(Hacl_Hash_SHA2_update_512(stream,input,1)==0);
    Hacl_Streaming_MD_state_64 *clone=Hacl_Hash_SHA2_copy_512(stream);
    assert(clone!=NULL);
    Hacl_Hash_SHA2_digest_512(stream,repeated);
    assert(Hacl_Hash_SHA2_update_512(stream,input+1,199)==0);
    Hacl_Hash_SHA2_digest_512(stream,repeated);
    assert(memcmp(digest,repeated,64)==0);
    assert(Hacl_Hash_SHA2_update_512(clone,input+1,63)==0);
    assert(Hacl_Hash_SHA2_update_512(clone,input+64,136)==0);
    Hacl_Hash_SHA2_digest_512(clone,repeated);
    assert(memcmp(digest,repeated,64)==0);
    Hacl_Hash_SHA2_reset_512(stream);
    assert(Hacl_Hash_SHA2_update_512(stream,abc,3)==0);
    Hacl_Hash_SHA2_digest_512(stream,repeated);
    check_hex(repeated,"ddaf35a193617abacc417349ae20413112e6fa4e89a97ea20a9eeee64b55d39a2192992a274fc1a836ba3c23a3feebbd454d4423643ce80e2a9ac94fa54ca49f",64);
    Hacl_Hash_SHA2_free_512(clone);
    Hacl_Hash_SHA2_free_512(stream);
    puts("sha512 vectors-stream-copy-reset-ok");
}

int main(void) {
#ifdef __dotcc__
    assert(__dotcc_is_little_endian());
#endif
    uint8_t storage[17]={0};
    uint64_t value=0x0102030405060708ULL;
    store64_be(storage+1,value);
    assert(load64_be(storage+1)==value);
    for (int i=1;i<=8;++i) assert(storage[i]==i);
    store64_le(storage+1,value);
    assert(load64_le(storage+1)==value);
    for (int i=1;i<=8;++i) assert(storage[i]==9-i);
    store32_be(storage+1,0x01020304U);
    assert(load32_be(storage+1)==0x01020304U);
    for (int i=1;i<=4;++i) assert(storage[i]==i);
    store32_le(storage+1,0x01020304U);
    assert(load32_le(storage+1)==0x01020304U);
    for (int i=1;i<=4;++i) assert(storage[i]==5-i);
    check_224(); check_256(); check_384(); check_512();
    puts("unaligned-little-and-big-endian-ok");
    return 0;
}
