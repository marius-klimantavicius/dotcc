#include "pinta.h"
#include <stdio.h>
#include <stdlib.h>

void *pinta_platform_file_open(void *, void *, uint32_t);
uint32_t pinta_platform_file_size(void *, void *);
uint32_t pinta_platform_file_read(void *, void *, void *, uint32_t);
void pinta_platform_file_close(void *, void *);

int main(void)
{
    PintaModule header = {0}, original;
    unsigned rejected = 0, unchanged = 0;
    PintaApiEnvironment environment = {0};
    PintaApiString name = {PINTA_STRING("invalid-opcode.pint"), 19, PINTA_API_ENCODING_UTF16};
    PintaApi *api;
    void *module;
    uint32_t status;
    _Static_assert(sizeof(PintaModule) == 84, "Pinned module header must be 84 bytes");
    header.magic = PINTA_CODE_MODULE_MAGIC;
    original = header;
    for (uint32_t length = 0; length < sizeof(PintaModule); length++) {
        header = original;
        if (pinta_module_init(&header, length) == NULL)
            rejected++;
        if (memcmp(&header, &original, sizeof(header)) == 0)
            unchanged++;
    }
    printf("short-headers rejected=%u total=84 unchanged=%u\n", rejected, unchanged);
    if (rejected != 84 || unchanged != 84)
        return 1;
    header = original;
    header.magic = 0xdeadbeef;
    unsigned bad_magic_rejected = pinta_module_init(&header, sizeof(header)) == NULL;
    printf("unknown-magic rejected=%u\n", bad_magic_rejected);
    if (!bad_magic_rejected)
        return 1;
    /* Mutate one field of a valid authored module at a time, using owned copies. */
    void *file = pinta_platform_file_open(NULL, PINTA_STRING("receipt.pint"), 12);
    if (file == NULL) return 2;
    uint32_t module_length = pinta_platform_file_size(NULL, file);
    if (module_length < sizeof(PintaModule) || module_length > 65536) {
        pinta_platform_file_close(NULL, file); return 2;
    }
    u8 *original_bytes = malloc(module_length), *working = malloc(module_length);
    if (original_bytes == NULL || working == NULL) {
        free(original_bytes); free(working); pinta_platform_file_close(NULL, file); return 2;
    }
    uint32_t read_length = pinta_platform_file_read(NULL, file, original_bytes, module_length);
    pinta_platform_file_close(NULL, file);
    if (read_length != module_length) { free(original_bytes); free(working); return 2; }
    memcpy(working, original_bytes, module_length);
    if (pinta_module_init(working, module_length) == NULL) {
        free(original_bytes); free(working); return 2;
    }
    struct { const char *name; unsigned index; } mutations[] = {
        {"strings_count", 10}, {"strings_offset", 11}, {"functions_count", 16},
        {"functions_offset", 17}, {"data_length", 19}, {"data_offset", 20}
    };
    for (unsigned index = 0; index < sizeof(mutations) / sizeof(mutations[0]); index++) {
        memcpy(working, original_bytes, module_length);
        ((uint32_t *)working)[mutations[index].index] = UINT32_MAX;
        unsigned rejected_field = pinta_module_init(working, module_length) == NULL;
        printf("header-field %s rejected=%u\n", mutations[index].name, rejected_field);
        if (!rejected_field) { free(original_bytes); free(working); return 1; }
    }
    free(original_bytes); free(working);
    environment.memory_length = 4 * 1024 * 1024;
    environment.memory = calloc(1, environment.memory_length);
    if (environment.memory == NULL)
        return 2;
    environment.heap_length = 1024 * 1024;
    environment.stack_length = 64 * 1024;
    environment.platform_encoding = PINTA_API_ENCODING_UTF16;
    environment.file_open = pinta_platform_file_open;
    environment.file_size = pinta_platform_file_size;
    environment.file_read = pinta_platform_file_read;
    environment.file_close = pinta_platform_file_close;
    api = pinta_api_create(&environment);
    if (api == NULL) { free(environment.memory); return 2; }
    module = api->load_module(api, &name);
    if (module == NULL) { free(environment.memory); return 3; }
    status = api->execute(api, module);
    printf("bad-opcode status=%u\n", status);
    free(environment.memory);
    return status == PINTA_EXCEPTION_INVALID_OPCODE ? 0 : 1;
}
