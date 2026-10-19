/* Test-only control. This executable is never referenced by a delivered project. */
#define ZSTD_STATIC_LINKING_ONLY
#include "zstd.h"
#include "zdict.h"
#include "zstd_errors.h"
#include <limits.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

unsigned long long dz_abi(int index);

/* Explicit checks remain active when the upstream release uses NDEBUG. */
#define CHECK(c) do { if (!(c)) { fprintf(stderr, "CHECK FAILED %s:%d: %s\n", __FILE__, __LINE__, #c); exit(1); } } while (0)
#define OK(c) do { size_t r_ = (c); if (ZSTD_isError(r_)) { fprintf(stderr, "%s: %s\n", #c, ZSTD_getErrorName(r_)); exit(1); } } while (0)

static unsigned char *read_file(const char *path, size_t *size) {
    FILE *f = fopen(path, "rb");
    long length;
    unsigned char *data;
    CHECK(f != NULL);
    CHECK(fseek(f, 0, SEEK_END) == 0);
    length = ftell(f);
    CHECK(length >= 0 && (unsigned long long)length <= SIZE_MAX);
    CHECK(fseek(f, 0, SEEK_SET) == 0);
    *size = (size_t)length;
    data = malloc(*size ? *size : 1);
    CHECK(data != NULL);
    CHECK(fread(data, 1, *size, f) == *size);
    CHECK(fclose(f) == 0);
    return data;
}

static void write_file(const char *path, const void *data, size_t size) {
    FILE *f = fopen(path, "wb");
    CHECK(f != NULL);
    CHECK(fwrite(data, 1, size, f) == size);
    CHECK(fclose(f) == 0);
}

static void path_in(char *out, size_t capacity, const char *directory, const char *name) {
    int n = snprintf(out, capacity, "%s/%s", directory, name);
    CHECK(n > 0 && (size_t)n < capacity);
}

static unsigned char *payload(size_t size) {
    unsigned char *p = malloc(size ? size : 1);
    size_t i;
    CHECK(p != NULL);
    for (i = 0; i < size; ++i)
        p[i] = (unsigned char)((i % 4093 < 3800) ? ("zstandard portable archive record\n"[i % 34]) : ((i * 37 + i / 251) & 255));
    return p;
}

static size_t compress_buffer(void *dst, size_t capacity, const void *src, size_t size,
                              const void *dict, size_t dict_size, int level) {
    ZSTD_CCtx *cctx = ZSTD_createCCtx();
    size_t result;
    CHECK(cctx != NULL);
    OK(ZSTD_CCtx_setParameter(cctx, ZSTD_c_compressionLevel, level));
    OK(ZSTD_CCtx_setParameter(cctx, ZSTD_c_checksumFlag, 1));
    OK(ZSTD_CCtx_setParameter(cctx, ZSTD_c_contentSizeFlag, 1));
    OK(ZSTD_CCtx_loadDictionary(cctx, dict, dict_size));
    result = ZSTD_compress2(cctx, dst, capacity, src, size);
    OK(ZSTD_freeCCtx(cctx));
    return result;
}

static void command_compress(const char *input, const char *output, const char *dict_path) {
    size_t size, dict_size = 0, bound, compressed;
    unsigned char *data = read_file(input, &size);
    unsigned char *dict = dict_path ? read_file(dict_path, &dict_size) : NULL;
    void *frame;
    bound = ZSTD_compressBound(size);
    frame = malloc(bound);
    CHECK(frame != NULL);
    compressed = compress_buffer(frame, bound, data, size, dict, dict_size, 3);
    OK(compressed);
    write_file(output, frame, compressed);
    free(frame); free(dict); free(data);
}

static void command_decompress(const char *input, const char *output, const char *dict_path) {
    size_t size, dict_size = 0, remaining = 1, pos = 0;
    unsigned char *data = read_file(input, &size);
    unsigned char *dict = dict_path ? read_file(dict_path, &dict_size) : NULL;
    ZSTD_DCtx *dctx = ZSTD_createDCtx();
    FILE *f = fopen(output, "wb");
    CHECK(dctx != NULL && f != NULL);
    OK(ZSTD_DCtx_loadDictionary(dctx, dict, dict_size));
    while (pos < size) {
        size_t chunk = size - pos < 997 ? size - pos : 997;
        ZSTD_inBuffer in = {data + pos, chunk, 0};
        while (in.pos < in.size) {
            unsigned char buffer[113];
            ZSTD_outBuffer out = {buffer, sizeof(buffer), 0};
            size_t before = in.pos;
            remaining = ZSTD_decompressStream(dctx, &out, &in);
            OK(remaining);
            CHECK(in.pos != before || out.pos != 0);
            CHECK(fwrite(buffer, 1, out.pos, f) == out.pos);
        }
        pos += in.pos;
    }
    CHECK(remaining == 0);
    CHECK(fclose(f) == 0);
    OK(ZSTD_freeDCtx(dctx));
    free(dict); free(data);
}

static size_t train(const void *samples, const size_t *sizes, unsigned count, void *dict, size_t capacity) {
    size_t n = ZDICT_trainFromBuffer(dict, capacity, samples, sizes, count);
    if (ZDICT_isError(n)) { fprintf(stderr, "train: %s\n", ZDICT_getErrorName(n)); exit(1); }
    CHECK(n != 0 && ZSTD_getDictID_fromDict(dict, n) != 0);
    return n;
}

static void command_train(const char *sample_path, const char *size_path, const char *output) {
    size_t sample_size, encoded_size, *sizes, total = 0, i, dict_size;
    unsigned char *samples = read_file(sample_path, &sample_size);
    unsigned char *encoded = read_file(size_path, &encoded_size);
    unsigned char dict[8192];
    CHECK(encoded_size != 0 && encoded_size % 8 == 0 && encoded_size / 8 <= UINT_MAX);
    sizes = malloc(encoded_size / 8 * sizeof(*sizes));
    CHECK(sizes != NULL);
    for (i = 0; i < encoded_size / 8; ++i) {
        uint64_t n = 0;
        unsigned b;
        for (b = 0; b < 8; ++b) n |= (uint64_t)encoded[i * 8 + b] << (8 * b);
        CHECK(n <= SIZE_MAX && n <= sample_size - total);
        sizes[i] = (size_t)n;
        total += (size_t)n;
    }
    CHECK(total == sample_size);
    dict_size = train(samples, sizes, (unsigned)(encoded_size / 8), dict, sizeof(dict));
    write_file(output, dict, dict_size);
    free(sizes); free(encoded); free(samples);
}

static void command_fixtures(const char *directory) {
    char path[4096], input[4096], dict_path[4096];
    size_t i, j, sizes[256], dict_size;
    unsigned char encoded[256 * 8], dict[8192];
    unsigned char *data = payload(262147);
    unsigned char *samples = malloc(256 * 1024);
    CHECK(samples != NULL);
    path_in(input, sizeof(input), directory, "payload.bin");
    write_file(input, data, 262147);
    path_in(path, sizeof(path), directory, "native.zst");
    command_compress(input, path, NULL);
    for (i = 0; i < 256; ++i) {
        sizes[i] = 1024;
        for (j = 0; j < 1024; ++j)
            samples[i * 1024 + j] = (unsigned char)(j % 97 < 80 ? data[(j + i * 13) % 262147] : (i * 17 + j));
        for (j = 0; j < 8; ++j) encoded[i * 8 + j] = (unsigned char)((uint64_t)sizes[i] >> (8 * j));
    }
    path_in(path, sizeof(path), directory, "samples.bin");
    write_file(path, samples, 256 * 1024);
    path_in(path, sizeof(path), directory, "sizes.bin");
    write_file(path, encoded, sizeof(encoded));
    dict_size = train(samples, sizes, 256, dict, sizeof(dict));
    path_in(dict_path, sizeof(dict_path), directory, "native.dict");
    write_file(dict_path, dict, dict_size);
    path_in(path, sizeof(path), directory, "native-dict.zst");
    command_compress(input, path, dict_path);
    free(samples); free(data);
}

static void stream_case(const unsigned char *data, size_t size) {
    size_t bound = ZSTD_compressBound(size) + 4096, used = 0, pos = 0, r = 1;
    unsigned char *frame = malloc(bound), *decoded = malloc(size);
    ZSTD_CCtx *cctx = ZSTD_createCCtx();
    ZSTD_DCtx *dctx = ZSTD_createDCtx();
    CHECK(frame && decoded && cctx && dctx);
    OK(ZSTD_CCtx_setParameter(cctx, ZSTD_c_checksumFlag, 1));
    OK(ZSTD_CCtx_setPledgedSrcSize(cctx, size));
    while (pos < size) {
        size_t chunk = size - pos < 997 ? size - pos : 997;
        ZSTD_inBuffer in = {data + pos, chunk, 0};
        while (in.pos < in.size) {
            ZSTD_outBuffer out = {frame + used, bound - used < 113 ? bound - used : 113, 0};
            r = ZSTD_compressStream2(cctx, &out, &in, ZSTD_e_continue);
            OK(r); used += out.pos;
        }
        pos += in.pos;
        if (pos == chunk) {
            do {
                ZSTD_inBuffer empty = {NULL, 0, 0};
                ZSTD_outBuffer out = {frame + used, bound - used < 113 ? bound - used : 113, 0};
                r = ZSTD_compressStream2(cctx, &out, &empty, ZSTD_e_flush);
                OK(r); used += out.pos;
            } while (r);
        }
    }
    do {
        ZSTD_inBuffer empty = {NULL, 0, 0};
        ZSTD_outBuffer out = {frame + used, bound - used < 113 ? bound - used : 113, 0};
        r = ZSTD_compressStream2(cctx, &out, &empty, ZSTD_e_end);
        OK(r); used += out.pos;
    } while (r);
    { ZSTD_inBuffer in = {frame, used, 0}; size_t written = 0;
      do {
          ZSTD_outBuffer out = {decoded + written, size - written < 127 ? size - written : 127, 0};
          r = ZSTD_decompressStream(dctx, &out, &in);
          OK(r); written += out.pos;
          CHECK(out.pos != 0 || r == 0 || in.pos < in.size);
      } while (r);
      CHECK(in.pos == used && written == size && memcmp(data, decoded, size) == 0);
    }
    OK(ZSTD_freeCCtx(cctx)); OK(ZSTD_freeDCtx(dctx));
    free(decoded); free(frame);
}

static void self_test(void) {
    size_t size = 262147, bound = ZSTD_compressBound(size), n, result, i;
    unsigned char *data = payload(size), *frame = malloc(bound), *decoded = malloc(size);
    ZSTD_CCtx *cctx = ZSTD_createCCtx();
    ZSTD_DCtx *dctx = ZSTD_createDCtx();
    int levels[] = {-5, 1, 3, 9, 19};
    CHECK(frame && decoded && cctx && dctx);
    CHECK(ZSTD_versionNumber() == ZSTD_VERSION_NUMBER);
    printf("CASE PASS version\n");
    for (i = 0; i < sizeof(levels) / sizeof(levels[0]); ++i) {
        n = compress_buffer(frame, bound, data, size, NULL, 0, levels[i]); OK(n);
        result = ZSTD_decompress(decoded, size, frame, n); OK(result);
        CHECK(result == size && memcmp(data, decoded, size) == 0);
    }
    printf("CASE PASS compression-strategies\n");
    n = compress_buffer(frame, bound, NULL, 0, NULL, 0, 3); OK(n);
    result = ZSTD_decompress(NULL, 0, frame, n); OK(result); CHECK(result == 0);
    printf("CASE PASS empty-frame\n");
    OK(ZSTD_CCtx_setParameter(cctx, ZSTD_c_enableLongDistanceMatching, 1));
    n = ZSTD_compress2(cctx, frame, bound, data, size); OK(n);
    result = ZSTD_decompress(decoded, size, frame, n); OK(result);
    CHECK(result == size && memcmp(data, decoded, size) == 0);
    printf("CASE PASS long-distance\n");
    stream_case(data, size);
    printf("CASE PASS streaming-flush-end\n");
    n = compress_buffer(frame, bound, data, size, NULL, 0, 3); OK(n);
    CHECK(ZSTD_getFrameContentSize(frame, n) == size);
    CHECK(ZSTD_findFrameCompressedSize(frame, n) == n);
    printf("CASE PASS frame-metadata\n");
    result = ZSTD_decompress(decoded, size - 1, frame, n);
    CHECK(ZSTD_isError(result) && ZSTD_getErrorCode(result) == ZSTD_error_dstSize_tooSmall);
    CHECK(ZSTD_isError(ZSTD_decompress(decoded, size, frame, n - 1)));
    frame[n - 1] ^= 1;
    result = ZSTD_decompress(decoded, size, frame, n);
    CHECK(ZSTD_isError(result) && ZSTD_getErrorCode(result) == ZSTD_error_checksum_wrong);
    CHECK(ZSTD_isError(ZSTD_decompress(decoded, size, "bad frame", 9)));
    printf("CASE PASS ordinary-errors\n");
    CHECK(ZSTD_isError(ZSTD_CCtx_setParameter(cctx, ZSTD_c_nbWorkers, 1)));
    printf("CASE PASS single-thread-profile\n");
    { unsigned char *samples = malloc(256 * 1024), dict[8192]; size_t sizes[256], dict_size;
      ZSTD_CDict *cdict; ZSTD_DDict *ddict;
      CHECK(samples != NULL);
      for (i = 0; i < 256; ++i) { size_t j; sizes[i] = 1024;
          for (j = 0; j < 1024; ++j) samples[i * 1024 + j] = (unsigned char)(j % 97 < 80 ? data[(j + i * 13) % size] : i * 17 + j);
      }
      dict_size = train(samples, sizes, 256, dict, sizeof(dict));
      cdict = ZSTD_createCDict(dict, dict_size, 3); ddict = ZSTD_createDDict(dict, dict_size);
      CHECK(cdict && ddict);
      n = ZSTD_compress_usingCDict(cctx, frame, bound, data, size, cdict); OK(n);
      CHECK(ZSTD_getDictID_fromFrame(frame, n) == ZSTD_getDictID_fromDict(dict, dict_size));
      result = ZSTD_decompress_usingDDict(dctx, decoded, size, frame, n, ddict); OK(result);
      CHECK(result == size && memcmp(data, decoded, size) == 0);
      CHECK(ZSTD_isError(ZSTD_decompress(decoded, size, frame, n)));
      OK(ZSTD_freeCDict(cdict)); OK(ZSTD_freeDDict(ddict)); free(samples);
    }
    printf("CASE PASS trained-prepared-dictionary\n");
    n = ZSTD_compress_usingDict(cctx, frame, bound, data, size, data, 1024, 3); OK(n);
    result = ZSTD_decompress_usingDict(dctx, decoded, size, frame, n, data, 1024); OK(result);
    CHECK(result == size && memcmp(data, decoded, size) == 0);
    printf("CASE PASS raw-dictionary\n");
    for (i = 0; i < 20; ++i) {
        OK(ZSTD_CCtx_reset(cctx, ZSTD_reset_session_and_parameters));
        OK(ZSTD_DCtx_reset(dctx, ZSTD_reset_session_and_parameters));
        n = ZSTD_compressCCtx(cctx, frame, bound, data, size, 1); OK(n);
        result = ZSTD_decompressDCtx(dctx, decoded, size, frame, n); OK(result);
        CHECK(result == size && memcmp(data, decoded, size) == 0);
    }
    printf("CASE PASS context-reuse-reset\n");
    OK(ZSTD_freeCCtx(cctx)); OK(ZSTD_freeDCtx(dctx));
    free(decoded); free(frame); free(data);
}

int main(int argc, char **argv) {
    if (argc == 2 && strcmp(argv[1], "self-test") == 0) self_test();
    else if (argc == 2 && strcmp(argv[1], "fail-control") == 0) CHECK(0 && "deliberate assertion control");
    else if (argc == 2 && strcmp(argv[1], "abi") == 0) {
        unsigned i; putchar('[');
        for (i = 0; i < 12; ++i) printf("%s%llu", i ? "," : "", dz_abi(i));
        puts("]");
    }
    else if (argc == 3 && strcmp(argv[1], "fixtures") == 0) command_fixtures(argv[2]);
    else if ((argc == 4 || argc == 5) && strcmp(argv[1], "compress") == 0)
        command_compress(argv[2], argv[3], argc == 5 ? argv[4] : NULL);
    else if ((argc == 4 || argc == 5) && strcmp(argv[1], "decompress") == 0)
        command_decompress(argv[2], argv[3], argc == 5 ? argv[4] : NULL);
    else if (argc == 5 && strcmp(argv[1], "train") == 0) command_train(argv[2], argv[3], argv[4]);
    else if (argc == 3 && strcmp(argv[1], "inspect") == 0) {
        size_t n; unsigned char *frame = read_file(argv[2], &n);
        printf("{\"contentSize\":%llu,\"dictionaryId\":%u,\"compressedSize\":%zu}\n",
               ZSTD_getFrameContentSize(frame, n), ZSTD_getDictID_fromFrame(frame, n), n);
        free(frame);
    }
    else { fprintf(stderr, "Expected self-test, fail-control, abi, fixtures DIR, compress/decompress INPUT OUTPUT [DICT], train SAMPLES SIZES OUTPUT, or inspect FRAME\n"); return 2; }
    return 0;
}
