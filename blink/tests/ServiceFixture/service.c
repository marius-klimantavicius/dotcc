/* Campaign-authored native Linux guest workload; not an emulator or host layer. */
#include <arpa/inet.h>
#include <errno.h>
#include <fcntl.h>
#include <poll.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/socket.h>
#include <unistd.h>

static char file_body[262144];
static char large_body[131072];

static int send_all(int fd, const char *data, size_t size) {
  while (size) {
    ssize_t count = send(fd, data, size, MSG_NOSIGNAL);
    if (count < 0 && errno == EINTR) continue;
    if (count <= 0) return -1;
    data += count;
    size -= (size_t)count;
  }
  return 0;
}

static int respond(int fd, const char *status, const char *body, size_t size) {
  char header[256];
  int count = snprintf(header, sizeof(header),
      "HTTP/1.1 %s\r\nContent-Type: text/plain\r\nContent-Length: %zu\r\n"
      "Connection: close\r\n\r\n", status, size);
  if (count < 0 || (size_t)count >= sizeof(header)) return -1;
  if (send_all(fd, header, (size_t)count)) return -1;
  return send_all(fd, body, size);
}

int main(int argc, char **argv) {
  char *end;
  unsigned long port;
  if (argc != 3) { fputs("usage: service PORT FILE\n", stderr); return 2; }
  errno = 0;
  port = strtoul(argv[1], &end, 10);
  if (errno || !*argv[1] || *end || port > 65535) return 2;
  int input = open(argv[2], O_RDONLY);
  if (input < 0) { perror("open fixture"); return 3; }
  size_t file_size = 0;
  for (;;) {
    ssize_t n = read(input, file_body + file_size, sizeof(file_body) - file_size);
    if (n < 0 && errno == EINTR) continue;
    if (n < 0) { perror("read fixture"); close(input); return 3; }
    if (!n) break;
    file_size += (size_t)n;
    if (file_size == sizeof(file_body)) {
      char extra;
      ssize_t more;
      do { more = read(input, &extra, 1); } while (more < 0 && errno == EINTR);
      if (more != 0) { fputs("fixture too large or unreadable\n", stderr); close(input); return 3; }
      break;
    }
  }
  close(input);
  for (size_t i = 0; i < sizeof(large_body); ++i) large_body[i] = 'a' + i % 26;

  int listener = socket(AF_INET, SOCK_STREAM, 0);
  if (listener < 0) { perror("socket"); return 4; }
  int reuse = 1;
  if (setsockopt(listener, SOL_SOCKET, SO_REUSEADDR, &reuse, sizeof(reuse))) {
    perror("setsockopt"); close(listener); return 4;
  }
  struct sockaddr_in address = {0};
  address.sin_family = AF_INET;
  address.sin_port = htons((unsigned short)port);
  address.sin_addr.s_addr = htonl(INADDR_LOOPBACK);
  if (bind(listener, (struct sockaddr *)&address, sizeof(address)) || listen(listener, 8)) {
    perror("bind/listen"); close(listener); return 4;
  }
  socklen_t address_size = sizeof(address);
  if (getsockname(listener, (struct sockaddr *)&address, &address_size)) {
    perror("getsockname"); close(listener); return 4;
  }
  printf("READY %u\n", (unsigned)ntohs(address.sin_port));
  fflush(stdout);
  int stopped = 0;
  while (!stopped) {
    struct pollfd ready = {listener, POLLIN, 0};
    int available;
    do { available = poll(&ready, 1, -1); } while (available < 0 && errno == EINTR);
    if (available < 0) { perror("poll"); close(listener); return 5; }
    int client = accept(listener, NULL, NULL);
    if (client < 0 && errno == EINTR) continue;
    if (client < 0) { perror("accept"); close(listener); return 5; }
    char request[4097];
    size_t size = 0;
    int complete = 0;
    while (size < sizeof(request) - 1) {
      ssize_t n = recv(client, request + size, sizeof(request) - 1 - size, 0);
      if (n < 0 && errno == EINTR) continue;
      if (n <= 0) break;
      size += (size_t)n;
      request[size] = 0;
      if (strstr(request, "\r\n\r\n")) { complete = 1; break; }
    }
    if (!complete) respond(client, "400 Bad Request", "bad request\n", 12);
    else if (!strncmp(request, "GET /health HTTP/1.1\r\n", 22))
      respond(client, "200 OK", "ok\n", 3);
    else if (!strncmp(request, "GET /file HTTP/1.1\r\n", 20))
      respond(client, "200 OK", file_body, file_size);
    else if (!strncmp(request, "GET /large HTTP/1.1\r\n", 21))
      respond(client, "200 OK", large_body, sizeof(large_body));
    else if (!strncmp(request, "POST /stop HTTP/1.1\r\n", 21)) {
      respond(client, "200 OK", "stopped\n", 8);
      stopped = 1;
    } else respond(client, "404 Not Found", "not found\n", 10);
    shutdown(client, SHUT_RDWR);
    close(client);
  }
  close(listener);
  puts("STOPPED");
  return 0;
}
