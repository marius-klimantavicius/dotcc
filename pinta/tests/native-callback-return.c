#include "pinta.h"
#include <stdio.h>
#include <stdlib.h>

void *pinta_platform_file_open(void *, void *, uint32_t);
uint32_t pinta_platform_file_size(void *, void *);
uint32_t pinta_platform_file_read(void *, void *, void *, uint32_t);
void pinta_platform_file_close(void *, void *);

static unsigned callback_calls;
static unsigned file_opens;
static unsigned file_reads;
static unsigned file_closes;

static void *tracked_open(void *context, void *name, uint32_t length)
{
    void *handle = pinta_platform_file_open(context, name, length);
    if (handle != NULL) file_opens++;
    return handle;
}

static uint32_t short_read(void *context, void *handle, void *buffer, uint32_t length)
{
    file_reads++;
    return pinta_platform_file_read(context, handle, buffer, length > 3 ? 3 : length);
}

static void tracked_close(void *context, void *handle)
{
    file_closes++;
    pinta_platform_file_close(context, handle);
}

static unsigned callback_moved;
static unsigned callback_payload_moved;
static wchar expected[] = {0x0104, 0xd83d, 0xde00, 0};

static PintaException return_after_compaction(PintaCore *core, PintaReference *arguments, PintaReference *result)
{
    PintaException status;
    PintaReference garbage = {0};
    uintptr_t before, payload_before;
    callback_calls++;
    if (arguments == NULL || arguments->reference == NULL || result == NULL)
        return PINTA_EXCEPTION_INVALID_ARGUMENTS;
    if (arguments->reference->block_kind != PINTA_KIND_ARRAY || pinta_array_ref_get_length(arguments) != 0)
        return PINTA_EXCEPTION_INVALID_ARGUMENTS;
    /* Leave a collectable gap before the result to make relocation observable. */
    status = pinta_lib_string_alloc(core, 64, &garbage);
    if (status != PINTA_OK)
        return status;
    garbage.reference = NULL;
    /* pinta_code_call_internal owns this result slot in its PINTA_GC_ENTER frame. */
    status = pinta_lib_string_alloc_copy(core, expected, 3, result);
    if (status != PINTA_OK)
        return status;
    before = (uintptr_t)result->reference;
    payload_before = (uintptr_t)pinta_string_ref_get_data(result);
    pinta_core_gc(core, 1);
    if (result->reference == NULL)
        return PINTA_EXCEPTION_ENGINE;
    callback_moved = before != (uintptr_t)result->reference;
    callback_payload_moved = payload_before != (uintptr_t)pinta_string_ref_get_data(result);
    if (result->reference == NULL || pinta_string_ref_get_length(result) != 3)
        return PINTA_EXCEPTION_ENGINE;
    if (memcmp(pinta_string_ref_get_data(result), expected, 3 * sizeof(wchar)) != 0)
        return PINTA_EXCEPTION_ENGINE;
    return PINTA_OK;
}

int main(void)
{
    PintaApiEnvironment environment = {0};
    PintaApi *api;
    PintaCore *core;
    PintaCoreInternalFunction *callbacks;
    void *module, *value = NULL;
    uint32_t status, length = 0;
    int exit_code = 1;
    PintaApiString name = {PINTA_STRING("callback-return.pint"), 20, PINTA_API_ENCODING_UTF16};
    PintaApiString answer = {PINTA_STRING("answer"), 6, PINTA_API_ENCODING_UTF16};
    environment.memory_length = 4 * 1024 * 1024;
    environment.memory = calloc(1, environment.memory_length);
    if (environment.memory == NULL)
        return 2;
    environment.heap_length = 1024 * 1024;
    environment.stack_length = 64 * 1024;
    environment.platform_encoding = PINTA_API_ENCODING_UTF16;
    environment.file_open = tracked_open;
    environment.file_size = pinta_platform_file_size;
    environment.file_read = short_read;
    environment.file_close = tracked_close;
    api = pinta_api_create(&environment);
    if (api == NULL)
        goto finish;
    core = api->core;
    callbacks = pinta_memory_alloc(core->memory, 2 * sizeof(PintaCoreInternalFunction));
    if (callbacks == NULL)
        goto finish;
    callbacks[0] = core->internal_functions[0];
    callbacks[1] = return_after_compaction;
    core->internal_functions = callbacks;
    core->internal_functions_length = 2;
    module = api->load_module(api, &name);
    if (module == NULL)
        goto finish;
    printf("files opens=%u reads=%u closes=%u\n", file_opens, file_reads, file_closes);
    if (file_opens != 1 || file_reads != 43 || file_closes != 1)
        goto finish;
    status = api->execute(api, module);
    printf("execute status=%u callbacks=%u moved=%u payload_moved=%u\n", status, callback_calls, callback_moved, callback_payload_moved);
    if (status != PINTA_OK)
        goto finish;
    status = api->unsafe_get_string(api, module, &answer, PINTA_API_ENCODING_UTF16, &length, &value);
    if (status != PINTA_OK || value == NULL)
        goto finish;
    printf("answer units=%u hex=", length);
    for (uint32_t index = 0; index < length; index++)
        printf("%04x", ((wchar *)value)[index]);
    printf("\n");
    if (callback_calls == 1 && callback_moved == 1 && callback_payload_moved == 1 && length == 3 && memcmp(value, expected, 3 * sizeof(wchar)) == 0)
        exit_code = 0;
finish:
    free(environment.memory);
    return exit_code;
}
