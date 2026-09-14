/* Compile against dotcc foundational headers plus campaign overrides. There is
 * intentionally no executable: the host functions must remain unresolved. */
#include <signal.h>
#include <setjmp.h>
#include <sys/uio.h>
#include <poll.h>
#include <termios.h>
#include <sys/socket.h>
#include <fcntl.h>
_Static_assert(sizeof(sigset_t) == 128, "signal mask storage");
_Static_assert(sizeof(pthread_t) == 8, "disabled-thread identity storage");
_Static_assert(sizeof(siginfo_t) == 128, "signal information storage");
_Static_assert(sizeof(struct sigaction) == 152, "signal action storage");
_Static_assert(sizeof(jmp_buf) == 200, "ordinary opaque unwind storage");
_Static_assert(sizeof(sigjmp_buf) == 336, "explicit signal unwind storage");
_Static_assert(sizeof(struct iovec) == 16, "scatter/gather storage");
_Static_assert(sizeof(struct pollfd) == 8, "readiness storage");
_Static_assert(sizeof(struct termios) == 60, "terminal attribute storage");
_Static_assert(sizeof(struct msghdr) == 56, "socket message storage");
_Static_assert(sizeof(struct cmsghdr) == 16, "ancillary header storage");
_Static_assert(sizeof(struct sockaddr_storage) == 128, "socket address storage");
_Static_assert(sizeof(struct flock) == 32, "file lock storage");
int check_declarations(sigset_t *mask, sigjmp_buf jump,
                       struct iovec *iov, struct pollfd *fds,
                       struct termios *terminal) {
  if (sigsetjmp(jump, 1)) siglongjmp(jump, 2);
  return sigprocmask(SIG_SETMASK, mask, 0) + poll(fds, 1, 0) +
         (int)readv(0, iov, 1) + tcgetattr(0, terminal) + socket(AF_INET, SOCK_STREAM, 0) + fcntl(0, F_GETFD);
}
