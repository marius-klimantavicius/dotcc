#define _GNU_SOURCE
#include <dlfcn.h>
#include <errno.h>
#include <fcntl.h>
#include <pthread.h>
#include <signal.h>
#include <stdio.h>
#include <string.h>
#include <sys/ioctl.h>
#include <termios.h>
#include <unistd.h>

static void result(const char *operation, int value, int error) {
  char text[200];
  int count = snprintf(text, sizeof(text), "%s result=%d errno=%d\n", operation, value, error);
  write(1, text, (size_t)count);
}
static void *thread(void *argument) { return argument; }

int main(int argc, char **argv) {
  const char *mode = argc > 1 ? argv[1] : "init";
  const char *library = argc > 2 ? argv[2] : "/dotnet/shared/Microsoft.NETCore.App/10.0.12/libSystem.Native.so";
  if (!strcmp(mode, "preflight")) {
    struct termios terminal;
    errno = 0; int rc = tcgetattr(0, &terminal); result("tcgetattr", rc, errno);
    int pair[2] = {-1, -1};
    errno = 0; rc = pipe2(pair, O_CLOEXEC); result("pipe2-cloexec", rc, errno);
    if (rc == 0) {
      errno = 0; rc = fcntl(pair[0], F_GETFD); result("fcntl-getfd", rc, errno);
      errno = 0; rc = fcntl(pair[0], F_GETFL); result("fcntl-getfl", rc, errno);
      errno = 0; rc = fcntl(pair[0], F_SETFL, O_NONBLOCK); result("fcntl-setfl", rc, errno);
      close(pair[0]); close(pair[1]);
    }
    struct sigaction action;
    errno = 0; rc = sigaction(SIGINT, NULL, &action); result("sigaction-query", rc, errno);
    pthread_attr_t attributes;
    errno = 0; rc = pthread_attr_init(&attributes); result("pthread-attr-init", rc, errno);
    if (!rc) {
      pthread_t id;
      errno = 0; rc = pthread_create(&id, &attributes, thread, NULL); result("pthread-create", rc, errno);
      pthread_attr_destroy(&attributes);
      if (!rc) { rc = pthread_join(id, NULL); result("pthread-join", rc, errno); }
    }
    return 0;
  }
  void *handle = dlopen(library, RTLD_NOW | RTLD_LOCAL);
  if (!handle) { const char *error = dlerror(); write(2, error, strlen(error)); return 2; }
  int (*initialize)(void) = (int (*)(void))dlsym(handle, "SystemNative_InitializeTerminalAndSignalHandling");
  if (!initialize) return 3;
  errno = 0;
  int rc = initialize(), error = errno;
  result("initialize-terminal-and-signals", rc, error);
  /* The native library owns a persistent signal worker after success. Keep the
     library loaded until process exit, as System.Console does. */
  return rc ? 0 : 1;
}
