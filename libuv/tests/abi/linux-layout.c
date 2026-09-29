#define _GNU_SOURCE
#include "uv.h"
#include <dirent.h>
#include <stddef.h>
#include <stdio.h>
#include <sys/epoll.h>
#include <sys/socket.h>
#define SIZE(T) printf("%s.size=%zu\n%s.align=%zu\n", #T, sizeof(T), #T, _Alignof(T))
#define OFFSET(T, F) printf("%s.%s=%zu\n", #T, #F, offsetof(T, F))
#define VALUE(V) printf("%s=%lld\n", #V, (long long) (V))
int main(void) {
  SIZE(long); SIZE(void*); SIZE(wchar_t); SIZE(size_t); SIZE(ssize_t);
  SIZE(off_t); SIZE(time_t); SIZE(uv_loop_t); SIZE(uv_buf_t);
#define TYPE_SIZE(_, name) SIZE(uv_##name##_t);
  UV_HANDLE_TYPE_MAP(TYPE_SIZE)
  UV_REQ_TYPE_MAP(TYPE_SIZE)
#undef TYPE_SIZE
  SIZE(pthread_t); SIZE(pthread_mutex_t); SIZE(pthread_rwlock_t);
  SIZE(pthread_cond_t); SIZE(pthread_once_t); SIZE(pthread_key_t); SIZE(sem_t);
  SIZE(struct stat); SIZE(struct dirent); SIZE(struct timespec);
  SIZE(struct epoll_event); SIZE(struct addrinfo); SIZE(struct sockaddr);
  SIZE(struct sockaddr_in); SIZE(struct sockaddr_in6); SIZE(struct sockaddr_storage);
  OFFSET(uv_buf_t, base); OFFSET(uv_buf_t, len);
  OFFSET(uv_loop_t, data); OFFSET(uv_loop_t, active_handles);
  OFFSET(uv_loop_t, handle_queue); OFFSET(uv_loop_t, active_reqs);
  OFFSET(uv_loop_t, internal_fields); OFFSET(uv_loop_t, backend_fd);
  OFFSET(uv_loop_t, watcher_queue); OFFSET(uv_loop_t, watchers);
  OFFSET(uv_loop_t, timer_heap); OFFSET(uv_loop_t, time);
  OFFSET(uv_handle_t, data); OFFSET(uv_handle_t, loop); OFFSET(uv_handle_t, type);
  OFFSET(uv_handle_t, close_cb); OFFSET(uv_handle_t, handle_queue);
  OFFSET(uv_req_t, data); OFFSET(uv_req_t, type);
  OFFSET(uv_fs_t, result); OFFSET(uv_fs_t, ptr); OFFSET(uv_fs_t, path);
  OFFSET(uv_fs_t, statbuf); OFFSET(uv_fs_t, work_req);
  OFFSET(uv_write_t, cb); OFFSET(uv_write_t, handle); OFFSET(uv_write_t, bufs);
  OFFSET(uv_tcp_t, io_watcher); OFFSET(uv_tcp_t, write_queue_size);
  OFFSET(struct epoll_event, events); OFFSET(struct epoll_event, data);
  OFFSET(struct stat, st_dev); OFFSET(struct stat, st_ino); OFFSET(struct stat, st_mode);
  OFFSET(struct stat, st_size); OFFSET(struct stat, st_atim);
  OFFSET(struct dirent, d_ino); OFFSET(struct dirent, d_type); OFFSET(struct dirent, d_name);
  OFFSET(struct addrinfo, ai_addrlen); OFFSET(struct addrinfo, ai_addr);
  OFFSET(struct addrinfo, ai_canonname); OFFSET(struct addrinfo, ai_next);
  VALUE(AF_INET); VALUE(AF_INET6); VALUE(SOCK_STREAM); VALUE(EPOLLIN);
  VALUE(EPOLLOUT); VALUE(EPOLLET); VALUE(UV_EBUSY); VALUE(UV_EOF); VALUE(UV_RUN_DEFAULT);
  return 0;
}
