/* Native-only finite peer. Never emitted as managed C or linked into its core. */
#include <arpa/inet.h>
#include <pthread.h>
#include <stdio.h>
#include <string.h>
#include <sys/socket.h>
#include <unistd.h>
int GuestTcpSetup(void), GuestTcpExchange(void), GuestTcpRelease(void);
int GuestTcpPort(void), GuestTcpPeerPort(void);
void GuestTcpDestroy(void);
static pthread_mutex_t mutex = PTHREAD_MUTEX_INITIALIZER;
static pthread_cond_t condition = PTHREAD_COND_INITIALIZER;
static int ready, peer_error, actual_peer_port;
#define CHECK(x) do { if (!(x)) { result = __LINE__; goto done; } } while (0)
static void Ready(int error) {
  pthread_mutex_lock(&mutex); if (error) peer_error = error;
  ready = 1; pthread_cond_signal(&condition); pthread_mutex_unlock(&mutex);
}
static void *Peer(void *unused) {
  (void)unused; int result = 0, fd = -1; unsigned char bytes[263];
  fd = socket(AF_INET, SOCK_STREAM, 0); CHECK(fd >= 0);
  struct sockaddr_in endpoint; memset(&endpoint, 0, sizeof(endpoint));
  endpoint.sin_family = AF_INET; endpoint.sin_port = htons(GuestTcpPort());
  endpoint.sin_addr.s_addr = htonl(INADDR_LOOPBACK);
  CHECK(!connect(fd, (struct sockaddr *)&endpoint, sizeof(endpoint)));
  socklen_t length = sizeof(endpoint);
  CHECK(!getsockname(fd, (struct sockaddr *)&endpoint, &length));
  actual_peer_port = ntohs(endpoint.sin_port); CHECK(actual_peer_port > 0);
  for (unsigned i = 0; i < 257; ++i) bytes[i] = (unsigned char)(i * 17 + 3);
  unsigned total = 0;
  while (total < 257) {
    ssize_t count = send(fd, bytes + total, 257 - total, MSG_NOSIGNAL);
    CHECK(count > 0 && count <= 257 - total); total += count;
  }
  CHECK(!shutdown(fd, SHUT_WR)); Ready(0);
  total = 0;
  while (total < 263) {
    ssize_t count = recv(fd, bytes + total, 263 - total, 0);
    CHECK(count > 0 && count <= 263 - total); total += count;
  }
  for (unsigned i = 0; i < 263; ++i) CHECK(bytes[i] == (unsigned char)(i * 29 + 11));
  CHECK(recv(fd, bytes, 1, 0) == 0);
done:
  if (fd >= 0 && close(fd) && !result) result = __LINE__;
  Ready(result); return 0;
}
int main(int argc, char **argv) {
  if (argc != 2) return 2;
  int result = GuestTcpSetup(); pthread_t peer; int started = 0;
  if (!result) {
    if (pthread_create(&peer, 0, Peer, 0)) result = __LINE__;
    else {
      started = 1; pthread_mutex_lock(&mutex);
      while (!ready) pthread_cond_wait(&condition, &mutex);
      result = peer_error; pthread_mutex_unlock(&mutex);
    }
  }
  if (!result) result = GuestTcpExchange();
  if (!result && started) {
    if (pthread_join(peer, 0)) result = __LINE__;
    else { started = 0; result = peer_error; }
  }
  if (!result) {
    if (GuestTcpPeerPort() != actual_peer_port) result = __LINE__;
    FILE *observed = fopen(argv[1], "w");
    if (!observed) result = __LINE__;
    else {
      fprintf(observed, "{\"kind\":\"native\",\"guest_port\":%d,\"guest_peer_port\":%d,\"physical_port\":%d,\"physical_peer_port\":%d,\"peer_response_bytes\":263,\"peer_exact\":true,\"peer_eof\":true}\n",
              GuestTcpPort(), GuestTcpPeerPort(), GuestTcpPort(), actual_peer_port);
      if (fclose(observed)) result = __LINE__;
    }
  }
  /* On a failed fixture the outer process-group timeout preserves evidence;
   * this runner does not inject transport cancellation or report success. */
  GuestTcpDestroy(); int cleanup = GuestTcpRelease();
  if (!result) result = cleanup;
  if (!result) puts("tcp peer response_bytes=263 exact=1 eof=1 joined=1");
  return result;
}
