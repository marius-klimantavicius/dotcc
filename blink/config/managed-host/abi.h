#ifndef BLINK_HOST_ABI_H
#define BLINK_HOST_ABI_H
#include <stddef.h>
#include <stdint.h>

/* Campaign C storage ABI, native-checked on Linux x64/glibc LP64.
 * These records contain no CLR object references and implement no services.
 * Guest Linux records remain the distinct upstream blink/linux.h records. */
typedef struct { uint64_t words[16]; } blink_host_sigset;
/* Upstream Machine retains this identity slot with DISABLE_THREADS. */
typedef uint64_t blink_host_thread_id;
typedef struct blink_host_siginfo {
  int32_t si_signo;
  int32_t si_errno;
  int32_t si_code;
  int32_t reserved_alignment;
  union {
    uint64_t alignment;
    unsigned char reserved[112];
    struct { void *address; } fault;
  } payload;
} blink_host_siginfo;

struct blink_host_sigaction {
  union {
    void (*simple)(int);
    void (*info)(int, blink_host_siginfo *, void *);
  } handler;
  blink_host_sigset mask;
  int32_t flags;
  void (*restorer)(void);
};

typedef struct {
  void *ss_sp;
  int32_t ss_flags;
  size_t ss_size;
} blink_host_signal_stack;

/* Storage parity only. A future managed unwind implementation must own its
 * token through an integer handle and capture/restore a virtual signal mask.
 * Native machine-register contents have no managed control-flow meaning. */
typedef struct {
  uint64_t native_register_storage[8];
  int32_t mask_saved;
  blink_host_sigset mask;
} blink_host_jump_storage;

struct blink_host_iovec { void *iov_base; size_t iov_len; };
struct blink_host_pollfd { int32_t fd; int16_t events; int16_t revents; };
struct blink_host_sockaddr { uint16_t sa_family; char sa_data[14]; };
struct blink_host_sockaddr_storage {
  uint16_t ss_family;
  unsigned char reserved[118];
  uint64_t alignment;
};
struct blink_host_msghdr {
  void *msg_name;
  uint32_t msg_namelen;
  struct blink_host_iovec *msg_iov;
  size_t msg_iovlen;
  void *msg_control;
  size_t msg_controllen;
  int32_t msg_flags;
};
struct blink_host_cmsghdr { size_t cmsg_len; int32_t cmsg_level; int32_t cmsg_type; };
struct blink_host_linger { int32_t l_onoff; int32_t l_linger; };
struct blink_host_ucred { int32_t pid; uint32_t uid; uint32_t gid; };
struct blink_host_termios {
  uint32_t c_iflag;
  uint32_t c_oflag;
  uint32_t c_cflag;
  uint32_t c_lflag;
  uint8_t c_line;
  uint8_t c_cc[32];
  uint32_t c_ispeed;
  uint32_t c_ospeed;
};
#endif
