/* Keep upstream SQLite unchanged. DOTCC_HOST_VFS enables explicit managed host
 * registration in the product; deterministic C fixtures use the memory default. */
#include "sqlite3.c"
#include "memory_vfs.c"
