/* Authored forwarding only; SQL/storage/error algorithms remain upstream.
 * This file is deliberately excluded from product publication until the
 * complete upstream closure and resumable host lifetime have been proven.
 */
#include "dotcc_pglite_boundary.h"
#include <unistd.h>

/* The selected fork does not publish a standalone libpglite header. These
 * declarations follow its pglitec.c, tcop/postgres.c, and libpq/pqcomm.c.
 */
extern void pgl_set_rw_cbs(dpg_io_callback read_callback, dpg_io_callback write_callback);
extern int pgl_setPGliteActive(int active);
extern void pgl_startPGlite(void);
extern void PostgresMainLoopOnce(void);
extern void PostgresMainLongJmp(void);
extern void pgl_pq_flush(void);
extern ssize_t pq_buffer_remaining_data(void);
extern void pgl_run_atexit_funcs(void);
extern void PostgresSendReadyForQueryIfNecessary(void);
extern int postmaster_alive_fds[2];

void dpg_set_io(dpg_io_callback read_callback, dpg_io_callback write_callback) {
    pgl_set_rw_cbs(read_callback, write_callback);
}
int dpg_start(dpg_postgres_main entry, int argc, char **argv) {
    pgl_setPGliteActive(1);
    return entry(argc, argv);
}
void dpg_attach_protocol(void) { pgl_startPGlite(); }
int dpg_open_liveness(void) { return pipe(postmaster_alive_fds); }
void dpg_step(void) { PostgresMainLoopOnce(); }
void dpg_recover_query_error(void) { PostgresMainLongJmp(); }
void dpg_flush(void) { pgl_pq_flush(); }
void dpg_ready(void) { PostgresSendReadyForQueryIfNecessary(); }
ssize_t dpg_buffered_input(void) { return pq_buffer_remaining_data(); }
void dpg_begin_shutdown(void) { pgl_setPGliteActive(0); }
void dpg_finish_cleanup(void) { pgl_run_atexit_funcs(); }
