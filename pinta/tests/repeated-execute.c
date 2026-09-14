/* Observe actual public API repeated-execution semantics without resetting VM state.
 * Link with the selected core and src/native-adapter.c; set PINTA_FIXTURES.
 * The process returns failure for an unusable first execution, and prints the
 * second execution observation for native/translated differential comparison.
 */
#include "pinta.h"
#include <stdio.h>
#include <stdlib.h>

void *pinta_platform_file_open(void *, void *, uint32_t);
uint32_t pinta_platform_file_size(void *, void *);
uint32_t pinta_platform_file_read(void *, void *, void *, uint32_t);
void pinta_platform_file_close(void *, void *);

static uint32_t report(PintaApi *api, void *module, const char *label)
{
    wchar key[] = {'W', 'O', 'R', 'D'};
    PintaApiString name = {key, 4, PINTA_API_ENCODING_UTF16};
    uint32_t length = 0, status;
    void *data = NULL;
    status = api->unsafe_get_string(api, module, &name, PINTA_API_ENCODING_UTF16, &length, &data);
    printf("%s get_status=%u length=%u units=", label, status, length);
    if (status == PINTA_API_OK && data != NULL)
        for (uint32_t i = 0; i < length; i++) printf("%04x", (unsigned)((wchar *)data)[i]);
    printf(" finished=%u\n", (unsigned)((PintaCore *)api->core)->threads->code_finished);
    return status;
}

int main(void)
{
    const uint32_t arena_length = 4 * 1024 * 1024;
    PintaApiEnvironment environment = {0};
    PintaApi *api;
    void *module;
    uint32_t status;
    wchar module_name[] = PINTA_STRING("unicode-string-v2.pint");
    wchar key[] = {'W', 'O', 'R', 'D'};
    wchar replacement[] = {'c', 'h', 'a', 'n', 'g', 'e', 'd'};
    PintaApiString name = {module_name, sizeof(module_name) / sizeof(wchar) - 1, PINTA_API_ENCODING_UTF16};
    PintaApiString global = {key, 4, PINTA_API_ENCODING_UTF16};
    PintaApiString value = {replacement, 7, PINTA_API_ENCODING_UTF16};
    environment.memory = calloc(1, arena_length);
    if (!environment.memory) return 2;
    environment.memory_length = arena_length;
    environment.heap_length = 1024 * 1024;
    environment.stack_length = 64 * 1024;
    environment.platform_encoding = PINTA_API_ENCODING_UTF16;
    environment.file_open = pinta_platform_file_open;
    environment.file_size = pinta_platform_file_size;
    environment.file_read = pinta_platform_file_read;
    environment.file_close = pinta_platform_file_close;
    api = pinta_api_create(&environment);
    if (!api) { free(environment.memory); return 3; }
    module = api->load_module(api, &name);
    if (!module) { free(environment.memory); return 4; }
    status = api->execute(api, module);
    printf("first execute_status=%u\n", status);
    if (status || report(api, module, "first")) { free(environment.memory); return 5; }
    status = api->set_string(api, module, &global, &value);
    if (status) { free(environment.memory); return 6; }
    status = api->execute(api, module);
    printf("second execute_status=%u\n", status);
    if (status || report(api, module, "second")) { free(environment.memory); return 7; }
    free(environment.memory);
    return 0;
}
