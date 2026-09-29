/* Native control for the same first useful workflow required of translated libuv. */
#include "uv.h"
#include <assert.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#define CHECK(call) do { int result_ = (call); if (result_ < 0) { \
  fprintf(stderr, "%s: %s\n", #call, uv_strerror(result_)); abort(); } } while (0)

static uv_loop_t loop;
static uv_tcp_t listener, client, accepted;
static uv_connect_t connect_req;
static uv_write_t client_write, server_write;
static uv_timer_t timer;
static uv_async_t async;
static uv_work_t work;
static uv_getaddrinfo_t resolver;
static uv_fs_t fs;
static uv_file file;
static char file_path[1024], file_bytes[128];
/* Two length-prefixed frames, with a NUL inside the second payload. */
static char payload[] = {0, 0, 0, 5, 'h', 'e', 'l', 'l', 'o', 0, 0, 0, 4, 'u', 'v', 0, '!'};
static char server_bytes[sizeof(payload)], client_bytes[sizeof(payload)];
static size_t server_count, client_count;
static int dns_done, work_done, async_done, tcp_done, file_done, writes, closes, file_stage;
static unsigned int work_result;
static uint64_t started;

static void closed(uv_handle_t* handle) { (void) handle; closes++; }
static void allocated(uv_handle_t* handle, size_t suggested, uv_buf_t* buffer) {
  (void) handle;
  buffer->base = malloc(suggested);
  assert(buffer->base != NULL);
  buffer->len = suggested;
}
static void written(uv_write_t* req, int status) {
  CHECK(status);
  writes++;
  if (req == &server_write)
    uv_close((uv_handle_t*) &accepted, closed);
}
static void received(uv_stream_t* stream, ssize_t nread, const uv_buf_t* buffer) {
  char* destination = stream == (uv_stream_t*) &client ? client_bytes : server_bytes;
  size_t* count = stream == (uv_stream_t*) &client ? &client_count : &server_count;
  assert(nread >= 0);
  if (nread > 0) {
    assert(*count + (size_t) nread <= sizeof(payload));
    memcpy(destination + *count, buffer->base, (size_t) nread);
    *count += (size_t) nread;
  }
  free(buffer->base);
  if (*count != sizeof(payload)) return;
  assert(memcmp(destination, payload, sizeof(payload)) == 0);
  CHECK(uv_read_stop(stream));
  if (stream == (uv_stream_t*) &accepted) {
    uv_buf_t data = uv_buf_init(server_bytes, sizeof(server_bytes));
    CHECK(uv_write(&server_write, stream, &data, 1, written));
  } else {
    tcp_done = 1;
    uv_close((uv_handle_t*) &client, closed);
    uv_close((uv_handle_t*) &listener, closed);
  }
}
static void connected(uv_connect_t* req, int status) {
  uv_buf_t data = uv_buf_init(payload, sizeof(payload));
  CHECK(status);
  CHECK(uv_read_start(req->handle, allocated, received));
  CHECK(uv_write(&client_write, req->handle, &data, 1, written));
}
static void connection(uv_stream_t* server, int status) {
  CHECK(status);
  CHECK(uv_tcp_init(&loop, &accepted));
  CHECK(uv_accept(server, (uv_stream_t*) &accepted));
  CHECK(uv_read_start((uv_stream_t*) &accepted, allocated, received));
}
static void resolved(uv_getaddrinfo_t* req, int status, struct addrinfo* results) {
  struct addrinfo* cursor;
  int found = 0;
  (void) req;
  CHECK(status);
  for (cursor = results; cursor; cursor = cursor->ai_next)
    if (cursor->ai_family == AF_INET || cursor->ai_family == AF_INET6) found++;
  assert(found > 0);
  uv_freeaddrinfo(results);
  dns_done = 1;
}
static void worked(uv_work_t* req) {
  unsigned int n;
  (void) req;
  for (n = 1; n <= 100; n++) work_result += n;
  CHECK(uv_async_send(&async));
}
static void after_work(uv_work_t* req, int status) {
  (void) req;
  CHECK(status);
  assert(work_result == 5050);
  work_done = 1;
}
static void notified(uv_async_t* handle) { (void) handle; async_done = 1; }
static void file_completed(uv_fs_t* req) {
  ssize_t result = req->result;
  uv_buf_t buffer;
  CHECK((int) result);
  if (file_stage == 0) {
    file = (uv_file) result;
    assert(strlen(req->path) < sizeof(file_path));
    strcpy(file_path, req->path);
  }
  uv_fs_req_cleanup(req);
  switch (file_stage++) {
    case 0:
      buffer = uv_buf_init(payload, sizeof(payload));
      CHECK(uv_fs_write(&loop, req, file, &buffer, 1, 0, file_completed));
      break;
    case 1:
      assert(result == sizeof(payload));
      buffer = uv_buf_init(file_bytes, sizeof(file_bytes));
      CHECK(uv_fs_read(&loop, req, file, &buffer, 1, 0, file_completed));
      break;
    case 2:
      assert(result == sizeof(payload));
      assert(memcmp(file_bytes, payload, sizeof(payload)) == 0);
      CHECK(uv_fs_close(&loop, req, file, file_completed));
      break;
    case 3:
      assert(result == 0);
      CHECK(uv_fs_unlink(&loop, req, file_path, file_completed));
      break;
    case 4:
      assert(result == 0);
      file_done = 1;
      break;
    default: abort();
  }
}
static void tick(uv_timer_t* handle) {
  assert(uv_now(&loop) - started < 10000);
  if (dns_done && work_done && async_done && tcp_done && file_done && writes == 2) {
    CHECK(uv_timer_stop(handle));
    uv_close((uv_handle_t*) handle, closed);
    uv_close((uv_handle_t*) &async, closed);
  }
}
int main(void) {
  struct sockaddr_in address;
  struct addrinfo hints;
  int size = sizeof(address);
  CHECK(uv_loop_init(&loop));
  CHECK(uv_tcp_init(&loop, &listener));
  CHECK(uv_ip4_addr("127.0.0.1", 0, &address));
  CHECK(uv_tcp_bind(&listener, (const struct sockaddr*) &address, 0));
  CHECK(uv_listen((uv_stream_t*) &listener, 8, connection));
  CHECK(uv_tcp_getsockname(&listener, (struct sockaddr*) &address, &size));
  CHECK(uv_tcp_init(&loop, &client));
  CHECK(uv_tcp_connect(&connect_req, &client, (const struct sockaddr*) &address, connected));
  CHECK(uv_async_init(&loop, &async, notified));
  CHECK(uv_queue_work(&loop, &work, worked, after_work));
  memset(&hints, 0, sizeof(hints));
  hints.ai_family = AF_UNSPEC;
  hints.ai_socktype = SOCK_STREAM;
  CHECK(uv_getaddrinfo(&loop, &resolver, resolved, "localhost", "80", &hints));
  CHECK(uv_fs_mkstemp(&loop, &fs, "dotcc-libuv-workflow-XXXXXX", file_completed));
  CHECK(uv_timer_init(&loop, &timer));
  started = uv_now(&loop);
  CHECK(uv_timer_start(&timer, tick, 1, 1));
  assert(uv_loop_close(&loop) == UV_EBUSY);
  assert(uv_run(&loop, UV_RUN_DEFAULT) == 0);
  assert(dns_done && work_done && async_done && tcp_done && file_done);
  assert(writes == 2 && closes == 5 && file_stage == 5);
  assert(!uv_loop_alive(&loop));
  CHECK(uv_loop_close(&loop));
  uv_library_shutdown();
  printf("{\"result\":\"passed\",\"version\":\"%s\",\"tcp_bytes\":%zu,\"file_bytes\":%zu,\"writes\":%d,\"closes\":%d,\"dns\":%d,\"work\":%u,\"async\":%d}\n",
         uv_version_string(), sizeof(payload), sizeof(payload), writes, closes, dns_done, work_result, async_done);
  return 0;
}
