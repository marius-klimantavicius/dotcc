#define _GNU_SOURCE 1
#include <dlfcn.h>
#include <errno.h>
#include <fcntl.h>
#include <pthread.h>
#include <stdint.h>
#include <stdlib.h>
#include <unistd.h>

/* ABI observations and forwarding only: no translated application bodies. */
typedef int (*hc_callback)(void *);
static _Thread_local int64_t hc_marker;
static const char *const failure_path = "/dev/null/roslynobj-errno-probe";

struct abi_fact { const char *name; int64_t value; };
static const struct abi_fact abi_facts[] = {
  { "pointer_size", sizeof(void *) }, { "int_size", sizeof(int) },
  { "int64_size", sizeof(int64_t) }, { "size_t_size", sizeof(size_t) },
  { "pthread_mutex_size", sizeof(pthread_mutex_t) },
  { "pthread_mutex_alignment", _Alignof(pthread_mutex_t) },
  { "pthread_mutexattr_size", sizeof(pthread_mutexattr_t) },
  { "pthread_mutexattr_alignment", _Alignof(pthread_mutexattr_t) },
  { "O_RDONLY", O_RDONLY }, { "ENOTDIR", ENOTDIR }, { "EINTR", EINTR },
  { "ERANGE", ERANGE }, { "EBUSY", EBUSY }, { "EDEADLK", EDEADLK },
  { "EPERM", EPERM }, { "PTHREAD_MUTEX_ERRORCHECK", PTHREAD_MUTEX_ERRORCHECK },
  { "main_marker", 0x16015 }, { "worker_marker", 0x22015 }
};
int hc_abi_count(void) { return (int)(sizeof abi_facts / sizeof abi_facts[0]); }
const char *hc_abi_name(int i) { return i >= 0 && i < hc_abi_count() ? abi_facts[i].name : NULL; }
int64_t hc_abi_value(int i) { return i >= 0 && i < hc_abi_count() ? abi_facts[i].value : 0; }
uintptr_t hc_tls_address(void) { return (uintptr_t)&hc_marker; }
int64_t hc_tls_read(void) { return hc_marker; }
void hc_tls_write(int64_t value) { hc_marker = value; }
void hc_set_errno(int value) { errno = value; }

/* errno is read in this native scope immediately after open, before returning. */
int hc_fail_capture(int *captured_errno, uintptr_t *captured_tls)
{
  errno = 0;
  int result = open(failure_path, O_RDONLY);
  int saved = errno;
  *captured_errno = saved;
  *captured_tls = (uintptr_t)__errno_location();
  return result;
}

enum { HC_PROTECTED = 0, HC_REENTRY = 1, HC_THREAD = 2, HC_MUTEX = 3 };
static const char *const protected_names[] = {
  "open_return", "native_errno_immediate", "native_errno_tls_before",
  "callback_return", "native_errno_after_callback", "native_errno_restored",
  "native_errno_tls_after", "native_marker_tls_before", "native_marker_tls_after"
};
static const char *const reentry_names[] = {
  "native_errno_before_callback", "native_errno_tls_before", "callback_return",
  "native_errno_after_callback", "native_errno_tls_after"
};
static const char *const thread_names[] = {
  "create_return", "join_return", "native_main_errno_tls_before",
  "native_main_errno_tls_after", "native_main_errno_before", "native_main_errno_after",
  "native_main_marker_tls_before", "native_main_marker_tls_after",
  "native_main_marker_before", "native_main_marker_after",
  "worker_entered", "worker_completed", "worker_errno_tls_before",
  "worker_errno_tls_after", "worker_errno_before_callback", "worker_errno_after_callback",
  "worker_marker_tls_before", "worker_marker_tls_after", "worker_marker_before",
  "worker_marker_after", "callback_return", "callback_context_must_remain_live"
};
static const char *const mutex_names[] = {
  "attr_init", "attr_settype", "mutex_init", "lock", "trylock_held",
  "lock_held_errorcheck", "unlock", "unlock_unowned", "mutex_destroy", "attr_destroy"
};
int hc_report_count(int kind)
{
  switch (kind) {
    case HC_PROTECTED: return (int)(sizeof protected_names / sizeof protected_names[0]);
    case HC_REENTRY: return (int)(sizeof reentry_names / sizeof reentry_names[0]);
    case HC_THREAD: return (int)(sizeof thread_names / sizeof thread_names[0]);
    case HC_MUTEX: return (int)(sizeof mutex_names / sizeof mutex_names[0]);
    default: return 0;
  }
}
const char *hc_report_name(int kind, int index)
{
  if (index < 0 || index >= hc_report_count(kind)) return NULL;
  switch (kind) {
    case HC_PROTECTED: return protected_names[index];
    case HC_REENTRY: return reentry_names[index];
    case HC_THREAD: return thread_names[index];
    case HC_MUTEX: return mutex_names[index];
    default: return NULL;
  }
}

/* Restores only errno, and only within this native scope. Not a product bridge. */
int hc_protected_probe(hc_callback callback, void *context, int64_t *out, size_t count)
{
  if (count != (size_t)hc_report_count(HC_PROTECTED)) return -1;
  errno = 0;
  int result = open(failure_path, O_RDONLY);
  int saved = errno;
  int *error_pointer = __errno_location();
  uintptr_t marker_pointer = (uintptr_t)&hc_marker;
  int callback_result = callback(context);
  int after_callback = errno;
  errno = saved;
  int restored = errno;
  out[0] = result; out[1] = saved; out[2] = (int64_t)(uintptr_t)error_pointer;
  out[3] = callback_result; out[4] = after_callback; out[5] = restored;
  out[6] = (int64_t)(uintptr_t)__errno_location();
  out[7] = (int64_t)marker_pointer; out[8] = (int64_t)(uintptr_t)&hc_marker;
  return 0;
}

/* Managed -> native -> managed reentry on the same native thread. */
int hc_reentry_probe(hc_callback callback, void *context, int error_value,
                     int64_t *out, size_t count)
{
  if (count != (size_t)hc_report_count(HC_REENTRY)) return -1;
  errno = error_value;
  int before = errno;
  int *error_pointer = __errno_location();
  int callback_result = callback(context);
  int after = errno;
  out[0] = before; out[1] = (int64_t)(uintptr_t)error_pointer;
  out[2] = callback_result; out[3] = after;
  out[4] = (int64_t)(uintptr_t)__errno_location();
  return 0;
}

struct worker_context {
  hc_callback callback;
  void *managed_context;
  int64_t values[11];
};
static void *worker_entry(void *opaque)
{
  struct worker_context *context = opaque;
  context->values[0] = 1;
  errno = EINTR;
  hc_marker = 0x22015;
  context->values[2] = (int64_t)(uintptr_t)__errno_location();
  context->values[4] = errno;
  context->values[6] = (int64_t)(uintptr_t)&hc_marker;
  context->values[8] = hc_marker;
  context->values[10] = context->callback(context->managed_context);
  context->values[5] = errno;
  context->values[3] = (int64_t)(uintptr_t)__errno_location();
  context->values[7] = (int64_t)(uintptr_t)&hc_marker;
  context->values[9] = hc_marker;
  context->values[1] = 1;
  return NULL;
}
int hc_thread_probe(hc_callback callback, void *managed_context, int64_t *out, size_t count)
{
  if (count != (size_t)hc_report_count(HC_THREAD)) return -1;
  for (size_t i = 0; i < count; ++i) out[i] = INT64_MIN;
  struct worker_context *context = calloc(1, sizeof *context);
  if (context == NULL) return -2;
  context->callback = callback;
  context->managed_context = managed_context;
  out[2] = (int64_t)(uintptr_t)__errno_location(); out[4] = errno;
  out[6] = (int64_t)(uintptr_t)&hc_marker; out[8] = hc_marker;
  pthread_t thread;
  int created = pthread_create(&thread, NULL, worker_entry, context);
  out[0] = created;
  if (created != 0) { free(context); out[21] = 0; return 0; }
  int joined = pthread_join(thread, NULL);
  out[1] = joined;
  out[3] = (int64_t)(uintptr_t)__errno_location(); out[5] = errno;
  out[7] = (int64_t)(uintptr_t)&hc_marker; out[9] = hc_marker;
  if (joined != 0) {
    /* Keep native context alive. Caller must retain the managed GCHandle too. */
    out[21] = 1;
    return 0;
  }
  for (int i = 0; i < 11; ++i) out[10 + i] = context->values[i];
  out[21] = 0;
  free(context);
  return 0;
}

/* Opaque pthread object layouts stay native; managed code sees return codes. */
int hc_mutex_probe(int64_t *out, size_t count)
{
  if (count != (size_t)hc_report_count(HC_MUTEX)) return -1;
  for (size_t i = 0; i < count; ++i) out[i] = INT64_MIN;
  pthread_mutexattr_t attribute;
  pthread_mutex_t mutex;
  out[0] = pthread_mutexattr_init(&attribute);
  if (out[0] != 0) return 0;
  out[1] = pthread_mutexattr_settype(&attribute, PTHREAD_MUTEX_ERRORCHECK);
  if (out[1] == 0) {
    out[2] = pthread_mutex_init(&mutex, &attribute);
    if (out[2] == 0) {
      out[3] = pthread_mutex_lock(&mutex);
      if (out[3] == 0) {
        out[4] = pthread_mutex_trylock(&mutex);
        out[5] = pthread_mutex_lock(&mutex);
        out[6] = pthread_mutex_unlock(&mutex);
        if (out[6] == 0) out[7] = pthread_mutex_unlock(&mutex);
      }
      out[8] = pthread_mutex_destroy(&mutex);
    }
  }
  out[9] = pthread_mutexattr_destroy(&attribute);
  return 0;
}

struct provider { const char *symbol; void *address; };
static const struct provider providers[] = {
  { "hc_abi_count", (void *)hc_abi_count }, { "open", (void *)open },
  { "__errno_location", (void *)__errno_location },
  { "pthread_create", (void *)pthread_create }, { "pthread_join", (void *)pthread_join },
  { "pthread_mutexattr_init", (void *)pthread_mutexattr_init },
  { "pthread_mutexattr_settype", (void *)pthread_mutexattr_settype },
  { "pthread_mutex_init", (void *)pthread_mutex_init },
  { "pthread_mutex_lock", (void *)pthread_mutex_lock },
  { "pthread_mutex_trylock", (void *)pthread_mutex_trylock },
  { "pthread_mutex_unlock", (void *)pthread_mutex_unlock },
  { "pthread_mutex_destroy", (void *)pthread_mutex_destroy },
  { "pthread_mutexattr_destroy", (void *)pthread_mutexattr_destroy },
  { "calloc", (void *)calloc }, { "free", (void *)free },
  { "dladdr", (void *)dladdr }
};
int hc_provider_count(void) { return (int)(sizeof providers / sizeof providers[0]); }
const char *hc_provider_symbol(int i) { return i >= 0 && i < hc_provider_count() ? providers[i].symbol : NULL; }
uintptr_t hc_provider_address(int i) { return i >= 0 && i < hc_provider_count() ? (uintptr_t)providers[i].address : 0; }
const char *hc_provider_path(int i)
{
  Dl_info info;
  if (i < 0 || i >= hc_provider_count() || dladdr(providers[i].address, &info) == 0) return NULL;
  return info.dli_fname;
}
uintptr_t hc_provider_base(int i)
{
  Dl_info info;
  if (i < 0 || i >= hc_provider_count() || dladdr(providers[i].address, &info) == 0) return 0;
  return (uintptr_t)info.dli_fbase;
}
