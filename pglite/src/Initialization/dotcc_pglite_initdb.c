/* Forwarding only. The original initdb main owns all catalog/config/BKI/SQL
 * algorithms. The host owns callback roots and catches its real exit status. */
#include "dotcc_pglite_initdb.h"

extern void pgl_set_system_fn(dpg_initdb_system_fn callback);
extern void pgl_set_popen_fn(dpg_initdb_popen_fn callback);
extern void pgl_set_pclose_fn(dpg_initdb_pclose_fn callback);

void
dpg_initdb_install(dpg_initdb_system_fn system_fn,
                   dpg_initdb_popen_fn popen_fn,
                   dpg_initdb_pclose_fn pclose_fn)
{
    pgl_set_system_fn(system_fn);
    pgl_set_popen_fn(popen_fn);
    pgl_set_pclose_fn(pclose_fn);
}

void
dpg_initdb_uninstall(void)
{
    pgl_set_system_fn(NULL);
    pgl_set_popen_fn(NULL);
    pgl_set_pclose_fn(NULL);
}

int
dpg_initdb_start(int (*entry)(int, char **), int argc, char **argv)
{
    return entry(argc, argv);
}
