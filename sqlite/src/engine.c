/* Keep upstream SQLite unchanged; compile the explicitly registered memory VFS
 * in the same translation unit so all callbacks have managed function pointers. */
#include "sqlite3.c"
#include "memory_vfs.c"
