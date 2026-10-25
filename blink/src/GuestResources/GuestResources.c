#include <errno.h>
#include <limits.h>
#include "blink/machine.h"
#include "blink/endian.h"
#include "HostMemory.h"
#include "GuestResources.h"

static int InitializeResourceLimits(struct System *s, size_t descriptors,
                                    size_t virtual_bytes, int explicit_virtual) {
  size_t bytes;
  if (!s) { errno = EFAULT; return -1; }
  bytes = BlinkHostMemoryLimit();
  if (!bytes) { errno = ENODEV; return -1; }
  if (!descriptors || descriptors > INT_MAX) { errno = EINVAL; return -1; }
  if (!explicit_virtual) virtual_bytes = bytes;
  if (explicit_virtual && (virtual_bytes < bytes || (virtual_bytes & 4095) ||
                          virtual_bytes > UINT64_C(4) * 1024 * 1024 * 1024)) {
    errno = EINVAL;
    return -1;
  }
  /* This is a startup operation, never an escape from a guest-lowered limit.
   * Refuse populated state and any previously changed resource record. */
  if (s->machines || s->fds.list || s->filemaps || s->cr3 || s->real ||
      s->vss || s->rss || s->loaded) { errno = EBUSY; return -1; }
  for (int i = 0; i < RLIM_NLIMITS_LINUX; ++i) {
    if (Read64(s->rlim[i].cur) != RLIM_INFINITY_LINUX ||
        Read64(s->rlim[i].max) != RLIM_INFINITY_LINUX) {
      errno = EBUSY; return -1;
    }
  }
  /* No fallible operation follows: publish all three records together on the
   * sole owning worker. Bytes remain bytes; upstream consumers divide by4096. */
  Write64(s->rlim[RLIMIT_AS_LINUX].cur, virtual_bytes);
  Write64(s->rlim[RLIMIT_AS_LINUX].max, virtual_bytes);
  Write64(s->rlim[RLIMIT_DATA_LINUX].cur, bytes);
  Write64(s->rlim[RLIMIT_DATA_LINUX].max, bytes);
  Write64(s->rlim[RLIMIT_NOFILE_LINUX].cur, descriptors);
  Write64(s->rlim[RLIMIT_NOFILE_LINUX].max, descriptors);
  return 0;
}

int BlinkHostInitializeResourceLimits(struct System *s, size_t descriptors) {
  return InitializeResourceLimits(s, descriptors, 0, 0);
}

int BlinkHostInitializeResourceLimitsWithVirtual(struct System *s,
                                                size_t descriptors,
                                                size_t virtual_bytes) {
  return InitializeResourceLimits(s, descriptors, virtual_bytes, 1);
}
