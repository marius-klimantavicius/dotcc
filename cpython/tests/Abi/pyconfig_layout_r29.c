#include <Python.h>
#include <stddef.h>
#include <stdio.h>

/* Same selected CPython/DotCC headers, never a mirrored PyConfig. */
int main(void)
{
#define FIELD(name) printf("\"" #name "\":%lu,", (unsigned long)offsetof(PyConfig, name))
    printf("{\"config_size\":%lu,\"preconfig_size\":%lu,\"wchar_size\":%lu,",
           (unsigned long)sizeof(PyConfig), (unsigned long)sizeof(PyPreConfig), (unsigned long)sizeof(wchar_t));
    FIELD(program_name); FIELD(stdlib_dir); FIELD(module_search_paths);
    FIELD(module_search_paths_set); FIELD(install_signal_handlers); FIELD(site_import);
    FIELD(parse_argv); FIELD(write_bytecode);
    printf("\"list_size\":%lu,\"list_length\":%lu,\"list_items\":%lu}\n",
           (unsigned long)sizeof(PyWideStringList), (unsigned long)offsetof(PyWideStringList, length),
           (unsigned long)offsetof(PyWideStringList, items));
    return sizeof(wchar_t) == 2 && (wchar_t)-1 > 0 ? 0 : 22;
}
