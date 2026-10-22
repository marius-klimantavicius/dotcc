#ifndef DOTCC_PGLITE_BOUNDARY_H
#define DOTCC_PGLITE_BOUNDARY_H
#include <stddef.h>
#include <sys/types.h>

/* Proposed forwarding boundary, not a delivered complete embedding API.
 * Every call enters the one static PostgreSQL program on its owning thread.
 * Buffers are borrowed only for the synchronous callback. Host callbacks must
 * not reenter or let managed exceptions escape into C. No opaque instance
 * handle is offered: this source profile has process-global PostgreSQL state.
 */
typedef ssize_t (*dpg_io_callback)(void *buffer, size_t length);
typedef int (*dpg_postgres_main)(int argc, char **argv);

/* Mirrors the fork's callback types, not Wasm's JavaScript iii signature. */
void dpg_set_io(dpg_io_callback read_callback, dpg_io_callback write_callback);

/* Requires generated catalogs, configured data/locale paths, and argv/strings
 * that remain alive across startup. Expected fork yield: exit 99, not a normal
 * C return. It is currently blocked by the host's terminal-exit semantics.
 * This cannot initialize a new cluster: upstream initdb has a separate closure.
 */
int dpg_start(dpg_postgres_main entry, int argc, char **argv);

/* Only after main has initialized and yielded 99. Sets dummy protocol port. */
void dpg_attach_protocol(void);

/* The read callback supplies PostgreSQL protocol frames, not SQL C strings.
 * Simple Query (Q) frames are the initial host target. The writer receives
 * binary protocol frames with explicit byte lengths. A SQL error may exit 100;
 * the host must call recovery, then drain/flush before accepting the next call.
 */
void dpg_step(void);
void dpg_recover_query_error(void);
void dpg_flush(void);
ssize_t dpg_buffered_input(void);

/* First disable embedding, then feed Terminate (X) and call dpg_step. The real
 * core's proc_exit must finish before cleanup callbacks are drained. These two
 * operations alone do not establish shutdown, durability, reset or reopen.
 * Never release callback buffers/roots before all cleanup has completed.
 */
void dpg_begin_shutdown(void);
void dpg_finish_cleanup(void);
#endif
