#include "pinta.h"
#include <stdio.h>
#include <stdlib.h>

int main(int argc, char **argv)
{
    PintaApiEnvironment environment = {0};
    PintaReference roots[128] = {{0}};
    PintaNativeFrame frame;
    PintaApi *api;
    PintaCore *core;
    PintaException status = PINTA_OK;
    unsigned count;
    int exit_code = 1;
    wchar external_text[] = {0x0104, 0x017d, 0};
    if (argc != 2)
        return 2;
    environment.memory_length = 64 * 1024;
    environment.memory = calloc(1, environment.memory_length);
    if (environment.memory == NULL)
        return 2;
    environment.heap_length = 1024;
    environment.stack_length = 1024;
    api = pinta_api_create(&environment);
    if (api == NULL)
        goto finish;
    core = api->core;
    frame.references = roots;
    frame.length = 128;
    frame.next = core->native;
    core->native = &frame;
    /* Every successful allocation remains in the real native root array. */
    for (count = 0; count < 127; count++) {
        status = pinta_lib_integer_alloc_value(core, (i32)(1000 + count), &roots[count]);
        if (status != PINTA_OK)
            break;
    }
    if (status != PINTA_EXCEPTION_OUT_OF_MEMORY || count == 0 || count == 127)
        goto release_roots;
    if (strcmp(argv[1], "string") == 0)
        status = pinta_lib_string_alloc_value(core, external_text, 2, &roots[127]);
    else if (strcmp(argv[1], "character") == 0)
        status = pinta_lib_char_alloc_value(core, 0x0104, &roots[127]);
    else if (strcmp(argv[1], "weak") == 0)
        status = pinta_lib_weak_alloc(core, &roots[127]);
    else
        goto release_roots;
    printf("value-exhaustion kind=%s retained=%u status=%u result_null=%u\n",
        argv[1], count, status, roots[127].reference == NULL);
    if (status == PINTA_EXCEPTION_OUT_OF_MEMORY && roots[127].reference == NULL)
        exit_code = 0;
release_roots:
    core->native = frame.next;
finish:
    free(environment.memory);
    return exit_code;
}
