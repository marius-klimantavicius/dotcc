#include <stdint.h>
#include <stddef.h>
#include <sys/socket.h>

/* Explicit-profile control-buffer traversal, with no socket/OS operation.
 * Header storage supplied by the caller must be readable. */
struct cmsghdr *blink_host_cmsg_nxthdr(const struct msghdr *message,
                                     const struct cmsghdr *current) {
  uintptr_t base, here;
  size_t offset, available, length, aligned;
  if (!message || !message->msg_control || !current) return 0;
  base = (uintptr_t)message->msg_control;
  here = (uintptr_t)current;
  if (here < base) return 0;
  offset = here - base;
  if (offset > message->msg_controllen) return 0;
  available = message->msg_controllen - offset;
  if (available < sizeof(struct cmsghdr)) return 0;
  length = current->cmsg_len;
  if (length < sizeof(struct cmsghdr) || length > available) return 0;
  if (length > SIZE_MAX - (sizeof(size_t) - 1)) return 0;
  aligned = (length + sizeof(size_t) - 1) & ~(sizeof(size_t) - 1);
  if (aligned > available || available - aligned < sizeof(struct cmsghdr)) return 0;
  if (here > UINTPTR_MAX - aligned) return 0;
  return (struct cmsghdr *)(here + aligned);
}
