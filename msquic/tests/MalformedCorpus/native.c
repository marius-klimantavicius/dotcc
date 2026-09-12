/* Selected decoder control. Microsoft upstream test values are MIT licensed;
 * exact attribution and explicitly derived mutations are in corpus.json. */
#include "precomp.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

typedef struct allocation { void* pointer; uint32_t tag; struct allocation* next; } allocation;
static allocation* allocations;
static unsigned live;
void* CxPlatAlloc(size_t size, uint32_t tag) {
    void* memory = malloc(size ? size : 1);
    if (!memory) return NULL;
    allocation* record = malloc(sizeof(*record));
    if (!record) { free(memory); return NULL; }
    *record = (allocation){memory, tag, allocations}; allocations = record; ++live;
    return memory;
}
void CxPlatFree(void* pointer, uint32_t tag) {
    if (!pointer) return;
    allocation** record = &allocations;
    while (*record && (*record)->pointer != pointer) record = &(*record)->next;
    if (!*record || (*record)->tag != tag) abort();
    allocation* previous = *record; *record = previous->next;
    free(pointer); free(previous); --live;
}
static void hex(const uint8_t* data, size_t length) {
    if (!length) { printf("-"); return; }
    for (size_t i = 0; i < length; ++i) printf("%02x", data[i]);
}
static void check(int value) { if (!value) abort(); }

int main(int argc, char** argv) {
    check(argc == 2);
    FILE* file = fopen(argv[1], "r"); check(file != NULL);
    char line[8192]; unsigned cases = 0;
    while (fgets(line, sizeof(line), file)) {
        char* id = strtok(line, "\t"); char* kind = strtok(NULL, "\t");
        char* server = strtok(NULL, "\t"); char* repeats = strtok(NULL, "\t");
        char* expected = strtok(NULL, "\t"); char* wire = strtok(NULL, "\t\r\n");
        check(id && kind && server && repeats && expected && wire);
        size_t length = strlen(wire) / 2; uint8_t bytes[2048]; check(length <= sizeof(bytes));
        for (size_t i = 0; i < length; ++i) { unsigned byte; check(sscanf(wire + 2*i, "%2x", &byte) == 1); bytes[i] = (uint8_t)byte; }
        BOOLEAN ok = FALSE; uint16_t offset = 1; uint64_t a = 0, b = 0, c = 0;
        const uint8_t* data = NULL; size_t data_length = 0;
        QUIC_TRANSPORT_PARAMETERS tp = {0};
        if (!strncmp(kind, "tp", 2)) {
            for (int n = 0; n < atoi(repeats); ++n) {
                ok = QuicCryptoTlsDecodeTransportParameters(NULL, (BOOLEAN)atoi(server), bytes, (uint16_t)length, &tp);
                check(ok == atoi(expected));
            }
            if (!strcmp(kind, "tp-replace")) {
                check(live == 1); uint8_t invalid = 255;
                ok = QuicCryptoTlsDecodeTransportParameters(NULL, (BOOLEAN)atoi(server), &invalid, 1, &tp);
                check(!ok && live == 0);
            }
            a = tp.Flags; b = tp.CibirLength; c = tp.CibirOffset;
            data = tp.VersionInfo; data_length = (size_t)tp.VersionInfoLength;
            printf("%s|%u|NA|%llu|%llu|%llu|", id, ok, (unsigned long long)a, (unsigned long long)b, (unsigned long long)c);
            hex(data, data_length); printf("|%u|", live);
            QuicCryptoTlsCleanupTransportParameters(&tp); check(live == 0);
            printf("0\n"); ++cases; continue;
        }
        if (!strcmp(kind, "reset")) {
            QUIC_RESET_STREAM_EX frame = {0}; ok = QuicResetStreamFrameDecode((uint16_t)length, bytes, &offset, &frame);
            if (ok) { a = frame.StreamID; b = frame.ErrorCode; c = frame.FinalSize; }
        } else if (!strcmp(kind, "stop")) {
            QUIC_STOP_SENDING_EX frame = {0}; ok = QuicStopSendingFrameDecode((uint16_t)length, bytes, &offset, &frame);
            if (ok) { a = frame.StreamID; b = frame.ErrorCode; }
        } else if (!strcmp(kind, "crypto")) {
            QUIC_CRYPTO_EX frame = {0}; ok = QuicCryptoFrameDecode((uint16_t)length, bytes, &offset, &frame);
            if (ok) { a = frame.Offset; b = frame.Length; c = (uint64_t)(frame.Data - bytes); data = frame.Data; data_length = (size_t)frame.Length; }
        } else if (!strcmp(kind, "maxdata")) {
            QUIC_MAX_DATA_EX frame = {0}; ok = QuicMaxDataFrameDecode((uint16_t)length, bytes, &offset, &frame);
            if (ok) a = frame.MaximumData;
        } else abort();
        check(ok == atoi(expected) && live == 0 && offset <= length);
        printf("%s|%u|%u|%llu|%llu|%llu|", id, ok, offset, (unsigned long long)a, (unsigned long long)b, (unsigned long long)c);
        hex(data, data_length); printf("|0|0\n"); ++cases;
    }
    check(!ferror(file) && live == 0); fclose(file);
    printf("PASS malformed corpus cases=%u allocations=0\n", cases);
    return 0;
}
