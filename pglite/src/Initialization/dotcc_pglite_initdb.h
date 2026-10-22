#ifndef DOTCC_PGLITE_INITDB_H
#define DOTCC_PGLITE_INITDB_H
#include <stdio.h>
#include <sys/types.h>

typedef ssize_t (*dpg_initdb_system_fn)(const char *command);
typedef FILE *(*dpg_initdb_popen_fn)(const char *command, const char *mode);
typedef int (*dpg_initdb_pclose_fn)(FILE *stream);

void dpg_initdb_install(dpg_initdb_system_fn system_fn,
                       dpg_initdb_popen_fn popen_fn,
                       dpg_initdb_pclose_fn pclose_fn);
void dpg_initdb_uninstall(void);
int dpg_initdb_start(int (*entry)(int, char **), int argc, char **argv);
#endif
