#include "probe.h"

struct StorageCell { int value; unsigned bits; };

/* A standalone record, rather than a top-level aggregate array. Its inline
   record array and every omitted row must be serialized into one payload. */
struct StorageRecord {
    struct StorageCell rows[129];
    unsigned tag;
};
static _Alignas(64) struct StorageRecord storage_record = {
    .rows = {[0]={-17, 0x80000000u}, [64]={(-2147483647 - 1), 0xa5a55a5au},
             [128]={23, 0xffffffffu}},
    .tag = 0x12345678u
};

/* Different size and bytes identify the standalone TLS integration path.
   The bitfields fill one 32-bit allocation unit without padding bits. */
struct StorageThreadRecord {
    unsigned words[256];
    unsigned a:3;
    unsigned b:9;
    signed c:7;
    unsigned d:13;
};
static _Alignas(128) _Thread_local struct StorageThreadRecord storage_thread_record = {
    .words = {[0]=73, [127]=0x80000001u, [255]=0xfedcba98u},
    .a=5, .b=0x12f, .c=-17, .d=0x1234
};

int storage_size(int table) {
    return table == 0 ? sizeof(storage_record) : sizeof(storage_thread_record);
}
unsigned char storage_byte(int table, int index) {
    if (table == 0) return ((unsigned char *)&storage_record)[index];
    return ((unsigned char *)&storage_thread_record)[index];
}
unsigned long storage_static_address(void) { return (unsigned long)&storage_record; }
unsigned long storage_tls_address(void) { return (unsigned long)&storage_thread_record; }
int storage_static_alignment(void) { return storage_static_address() % 64 == 0; }
int storage_tls_alignment(void) { return storage_tls_address() % 128 == 0; }

int storage_static_check(int first) {
    if (storage_record.tag != 0x12345678u) return 0;
    for (int i=0; i<129; ++i) {
        int value = i == 0 ? first : i == 64 ? (-2147483647 - 1) : i == 128 ? 23 : 0;
        unsigned bits = i == 0 ? 0x80000000u : i == 64 ? 0xa5a55a5au : i == 128 ? 0xffffffffu : 0;
        if (storage_record.rows[i].value != value || storage_record.rows[i].bits != bits) return 0;
    }
    return 1;
}
void storage_static_set(int first) { storage_record.rows[0].value = first; }

int storage_tls_check(unsigned first) {
    if (storage_thread_record.a != 5 || storage_thread_record.b != 0x12f ||
        storage_thread_record.c != -17 || storage_thread_record.d != 0x1234) return 0;
    for (int i=0; i<256; ++i) {
        unsigned word = i == 0 ? first : i == 127 ? 0x80000001u : i == 255 ? 0xfedcba98u : 0;
        if (storage_thread_record.words[i] != word) return 0;
    }
    return 1;
}
unsigned storage_tls_step(void) { return storage_thread_record.words[0]++; }

static int storage_local_check(struct StorageCell *rows, int aligned) {
    int count = aligned ? 129 : 128;
    int middle = aligned ? 65 : 64;
    for (int i=0; i<count; ++i) {
        int value = aligned ? (i == 0 ? -53 : i == middle ? 59 : i == count-1 ? 61 : 0)
                            : (i == 0 ? 31 : i == middle ? -41 : i == count-1 ? 47 : 0);
        unsigned bits = aligned ? (i == 0 ? 0x76543210u : i == middle ? 0xfffffffeu : i == count-1 ? 0x10203040u : 0)
                                : (i == 0 ? 0x01020304u : i == middle ? 0x80000002u : i == count-1 ? 0xaabbccddu : 0);
        if (rows[i].value != value || rows[i].bits != bits) return 0;
    }
    return 1;
}

int storage_loop(int rounds, int depth) {
    for (int pass=0; pass<rounds; ++pass) {
        struct StorageCell loop_rows[128] = {
            [0]={31, 0x01020304u}, [64]={-41, 0x80000002u}, [127]={47, 0xaabbccddu}
        };
        _Alignas(64) struct StorageCell loop_aligned[129] = {
            [0]={-53, 0x76543210u}, [65]={59, 0xfffffffeu}, [128]={61, 0x10203040u}
        };
        if ((unsigned long)loop_aligned % 64 != 0 || &loop_rows[0] == &loop_aligned[0] ||
            !storage_local_check(loop_rows, 0) || !storage_local_check(loop_aligned, 1)) return 0;

        /* Poison every member, including initially omitted rows. Re-entering
           this lexical declaration must restore the ENTIRE constant payload. */
        int stamp = 100 + pass + depth * 10;
        for (int i=0; i<128; ++i) { loop_rows[i].value = stamp; loop_rows[i].bits = 0xdeadbeefu; }
        if (!storage_local_check(loop_aligned, 1)) return 0;
        for (int i=0; i<129; ++i) { loop_aligned[i].value = -stamp; loop_aligned[i].bits = 0xcafebabeu; }

        /* Both caller arrays stay live across recursive invocation of the same
           declarations; concurrent managed workers exercise the same property. */
        if (pass == 1 && depth > 0 && !storage_loop(3, depth-1)) return 0;
        for (int i=0; i<128; ++i)
            if (loop_rows[i].value != stamp || loop_rows[i].bits != 0xdeadbeefu) return 0;
        for (int i=0; i<129; ++i)
            if (loop_aligned[i].value != -stamp || loop_aligned[i].bits != 0xcafebabeu) return 0;
    }
    return 1;
}
