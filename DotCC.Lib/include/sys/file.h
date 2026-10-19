#ifndef _SYS_FILE_H
#define _SYS_FILE_H

/* Real Linux x64 open-file-description advisory locks. The shared runtime
   borrows the backing OS file handle; unsupported backings/hosts fail with
   ENOTSUP. These locks are distinct from fcntl's process-owned record locks. */

#define LOCK_SH 1
#define LOCK_EX 2
#define LOCK_NB 4
#define LOCK_UN 8

int flock(int fd, int operation);

#endif /* _SYS_FILE_H */
