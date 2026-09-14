/* Linux monotonic-time reference for the fixed receipt workload.
 * Modules are read into immutable host memory once; each file-open has its own cursor.
 */
#include "pinta.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include <sys/resource.h>

typedef struct { unsigned char *bytes; uint32_t length; } Input;
typedef struct { Input *input; uint32_t position; } Cursor;
PintaException pinta_api_set_builtin_objects(PintaCore *, PintaModuleDomain *);
static void *open_input(void *context, void *name, uint32_t length) {
    (void)name; (void)length;
    Cursor *cursor = malloc(sizeof(Cursor));
    if (cursor) { cursor->input = context; cursor->position = 0; }
    return cursor;
}
static uint32_t input_size(void *context, void *handle) { (void)context; return ((Cursor *)handle)->input->length; }
static uint32_t read_input(void *context, void *handle, void *output, uint32_t length) {
    (void)context; Cursor *cursor = handle;
    uint32_t remaining = cursor->input->length - cursor->position;
    if (length > remaining) length = remaining;
    memcpy(output, cursor->input->bytes + cursor->position, length);
    cursor->position += length;
    return length;
}
static void close_input(void *context, void *handle) { (void)context; free(handle); }
static double now(void) {
    struct timespec value;
    if (clock_gettime(CLOCK_MONOTONIC, &value)) exit(2);
    return (double)value.tv_sec + (double)value.tv_nsec / 1000000000.0;
}
static PintaApiString text(wchar *value) {
    PintaApiString result = {value, 0, PINTA_API_ENCODING_UTF16};
    while (value[result.string_length]) result.string_length++;
    return result;
}
int main(int argc, char **argv) {
    if (argc != 3) return 2;
    int count = atoi(argv[2]); if (count < 1 || count > 10000) return 2;
    FILE *file = fopen(argv[1], "rb"); if (!file) return 2;
    if (fseek(file, 0, SEEK_END)) return 2;
    long length = ftell(file); if (length < 0 || (unsigned long)length > UINT32_MAX) return 2;
    rewind(file);
    Input input = {malloc((size_t)length), (uint32_t)length};
    if (!input.bytes || fread(input.bytes, 1, (size_t)length, file) != (size_t)length) return 2;
    fclose(file);
    double times[6] = {0};
    const char *phases[] = {"create", "load_and_globals", "execute", "collect_compact", "copy_output", "dispose"};
    wchar expected[] = PINTA_STRING("Customer: Ada\nTotal: 37.5\n");
    for (int iteration = -5; iteration < count; iteration++) {
        if (!iteration) memset(times, 0, sizeof(times));
        double start = now();
        unsigned char *allocation = calloc(1, 4194304 + 64);
        if (!allocation) return 2;
        memset(allocation, 0xa5, 32); memset(allocation + 32 + 4194304, 0xa5, 32);
        PintaApiEnvironment env = {0};
        env.memory = allocation + 32; env.memory_length = 4194304;
        env.heap_length = 1048576; env.stack_length = 65536;
        env.platform_encoding = PINTA_API_ENCODING_UTF16; env.environment_context = &input;
        env.file_open = open_input; env.file_size = input_size; env.file_read = read_input; env.file_close = close_input;
        PintaApi *api = pinta_api_create(&env); if (!api) return 3;
        double created = now();
        PintaApiString name = text(PINTA_STRING("receipt.pint"));
        void *module = api->load_module(api, &name); if (!module) return 4;
        /* The owning facade installs the same upstream require builtin on load. */
        if (pinta_api_set_builtin_objects((PintaCore *)api->core, (PintaModuleDomain *)module)) return 4;
        name = text(PINTA_STRING("customer")); PintaApiString value = text(PINTA_STRING("Ada"));
        if (api->set_string(api, module, &name, &value)) return 5;
        name = text(PINTA_STRING("quantity")); if (api->set_integer(api, module, &name, 3)) return 5;
        name = text(PINTA_STRING("unitPrice")); value = text(PINTA_STRING("12.50"));
        if (api->set_string(api, module, &name, &value)) return 5;
        double loaded = now();
        if (api->execute(api, module)) return 6;
        double executed = now();
        pinta_core_gc((PintaCore *)api->core, 1);
        double collected = now();
        uint32_t size = 0; void *bytes = NULL;
        if (api->unsafe_get_output_buffer(api, &size, &bytes)) return 7;
        unsigned char *copy = malloc(size); if (!copy) return 2;
        memcpy(copy, bytes, size);
        double copied = now();
        if (size != sizeof(expected) - sizeof(wchar) || memcmp(copy, expected, size)) return 8;
        for (unsigned i = 0; i < 32; i++)
            if (allocation[i] != 0xa5 || allocation[32 + 4194304 + i] != 0xa5) return 9;
        double dispose_start = now(); free(copy); free(allocation); double disposed = now();
        times[0] += created - start; times[1] += loaded - created; times[2] += executed - loaded;
        times[3] += collected - executed; times[4] += copied - collected; times[5] += disposed - dispose_start;
    }
    printf("iterations=%d\n", count);
    for (unsigned phase = 0; phase < 6; phase++) printf("%s_seconds=%.9f\n", phases[phase], times[phase]);
    struct rusage usage; if (getrusage(RUSAGE_SELF, &usage)) return 2;
    printf("peak_working_set_bytes=%llu\n", (unsigned long long)usage.ru_maxrss * 1024);
    printf("arena_bytes=4194304\nheap_bytes=1048576\nstack_bytes=65536\noutput_bytes=52\nvalidated=1\n");
    free(input.bytes); return 0;
}
