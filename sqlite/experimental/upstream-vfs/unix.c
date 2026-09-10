/* Compile the real amalgamated os_unix.c, with explicit OS bindings.
 * This is a separate experimental engine; HostVfs remains the default product. */
#include "unix-bindings.h"
#include "sqlite3.c"
#include "host_mutex.c"
