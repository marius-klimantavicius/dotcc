#define _GNU_SOURCE 1
#include <stddef.h>
#include <stdio.h>
#ifndef BLINK_HOST_STORAGE_ONLY
#include <signal.h>
#include <setjmp.h>
#include <sys/uio.h>
#include <poll.h>
#include <termios.h>
#include <pthread.h>
#include <sys/socket.h>
#endif
#include "abi.h"

static int failures;
#define CHECK(label, native, profile) do { \
  size_t n = (native), p = (profile); \
  printf("%s %zu %zu\n", label, n, p); \
  if (n != p) ++failures; \
} while (0)
#ifdef BLINK_HOST_STORAGE_ONLY
/* Identical labels/cases, evaluating only the authored profile records. The
 * runner compares this output with the separately compiled native oracle. */
#define SIZE(n, p) printf("%s.size %zu\n", #n, sizeof(p)); \
                   printf("%s.alignment %zu\n", #n, _Alignof(p))
#define OFFSET(n, f, p, pf) printf("%s.%s %zu\n", #n, #f, offsetof(p, pf))
#else
#define SIZE(n, p) CHECK(#n ".size", sizeof(n), sizeof(p)); \
                   CHECK(#n ".alignment", _Alignof(n), _Alignof(p))
#define OFFSET(n, f, p, pf) CHECK(#n "." #f, offsetof(n, f), offsetof(p, pf))
#endif
int main(void) {
  SIZE(sigset_t, blink_host_sigset);
  SIZE(pthread_t, blink_host_thread_id);
  SIZE(siginfo_t, blink_host_siginfo);
  OFFSET(siginfo_t, si_signo, blink_host_siginfo, si_signo);
  OFFSET(siginfo_t, si_errno, blink_host_siginfo, si_errno);
  OFFSET(siginfo_t, si_code, blink_host_siginfo, si_code);
  OFFSET(siginfo_t, si_addr, blink_host_siginfo, payload.fault.address);
  SIZE(struct sigaction, struct blink_host_sigaction);
  OFFSET(struct sigaction, sa_handler, struct blink_host_sigaction, handler.simple);
  OFFSET(struct sigaction, sa_sigaction, struct blink_host_sigaction, handler.info);
  OFFSET(struct sigaction, sa_mask, struct blink_host_sigaction, mask);
  OFFSET(struct sigaction, sa_flags, struct blink_host_sigaction, flags);
  OFFSET(struct sigaction, sa_restorer, struct blink_host_sigaction, restorer);
  SIZE(stack_t, blink_host_signal_stack);
  OFFSET(stack_t, ss_sp, blink_host_signal_stack, ss_sp);
  OFFSET(stack_t, ss_flags, blink_host_signal_stack, ss_flags);
  OFFSET(stack_t, ss_size, blink_host_signal_stack, ss_size);
  SIZE(sigjmp_buf, blink_host_jump_storage);
  SIZE(jmp_buf, blink_host_jump_storage);
  SIZE(struct iovec, struct blink_host_iovec);
  OFFSET(struct iovec, iov_base, struct blink_host_iovec, iov_base);
  OFFSET(struct iovec, iov_len, struct blink_host_iovec, iov_len);
  SIZE(struct pollfd, struct blink_host_pollfd);
  OFFSET(struct pollfd, fd, struct blink_host_pollfd, fd);
  OFFSET(struct pollfd, events, struct blink_host_pollfd, events);
  OFFSET(struct pollfd, revents, struct blink_host_pollfd, revents);
  SIZE(nfds_t, uint64_t);
  SIZE(struct termios, struct blink_host_termios);
  OFFSET(struct termios, c_iflag, struct blink_host_termios, c_iflag);
  OFFSET(struct termios, c_oflag, struct blink_host_termios, c_oflag);
  OFFSET(struct termios, c_cflag, struct blink_host_termios, c_cflag);
  OFFSET(struct termios, c_lflag, struct blink_host_termios, c_lflag);
  OFFSET(struct termios, c_line, struct blink_host_termios, c_line);
  OFFSET(struct termios, c_cc, struct blink_host_termios, c_cc);
  OFFSET(struct termios, c_ispeed, struct blink_host_termios, c_ispeed);
  OFFSET(struct termios, c_ospeed, struct blink_host_termios, c_ospeed);
  SIZE(cc_t, uint8_t);
  SIZE(tcflag_t, uint32_t);
  SIZE(speed_t, uint32_t);
  SIZE(socklen_t, uint32_t);
  SIZE(sa_family_t, uint16_t);
  SIZE(struct sockaddr, struct blink_host_sockaddr);
  OFFSET(struct sockaddr, sa_family, struct blink_host_sockaddr, sa_family);
  OFFSET(struct sockaddr, sa_data, struct blink_host_sockaddr, sa_data);
  SIZE(struct sockaddr_storage, struct blink_host_sockaddr_storage);
  OFFSET(struct sockaddr_storage, ss_family, struct blink_host_sockaddr_storage, ss_family);
  SIZE(struct msghdr, struct blink_host_msghdr);
  OFFSET(struct msghdr, msg_name, struct blink_host_msghdr, msg_name);
  OFFSET(struct msghdr, msg_namelen, struct blink_host_msghdr, msg_namelen);
  OFFSET(struct msghdr, msg_iov, struct blink_host_msghdr, msg_iov);
  OFFSET(struct msghdr, msg_iovlen, struct blink_host_msghdr, msg_iovlen);
  OFFSET(struct msghdr, msg_control, struct blink_host_msghdr, msg_control);
  OFFSET(struct msghdr, msg_controllen, struct blink_host_msghdr, msg_controllen);
  OFFSET(struct msghdr, msg_flags, struct blink_host_msghdr, msg_flags);
  SIZE(struct cmsghdr, struct blink_host_cmsghdr);
  OFFSET(struct cmsghdr, cmsg_len, struct blink_host_cmsghdr, cmsg_len);
  OFFSET(struct cmsghdr, cmsg_level, struct blink_host_cmsghdr, cmsg_level);
  OFFSET(struct cmsghdr, cmsg_type, struct blink_host_cmsghdr, cmsg_type);
  SIZE(struct linger, struct blink_host_linger);
  OFFSET(struct linger, l_onoff, struct blink_host_linger, l_onoff);
  OFFSET(struct linger, l_linger, struct blink_host_linger, l_linger);
  SIZE(struct ucred, struct blink_host_ucred);
  OFFSET(struct ucred, pid, struct blink_host_ucred, pid);
  OFFSET(struct ucred, uid, struct blink_host_ucred, uid);
  OFFSET(struct ucred, gid, struct blink_host_ucred, gid);
  return failures != 0;
}
