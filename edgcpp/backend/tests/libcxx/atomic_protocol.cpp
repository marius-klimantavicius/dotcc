#include <__atomic/atomic_sync.h>
#include <cstdint>
#include <__functional/hash.h>
extern "C" unsigned atomic_bucket(void* address) {
  return std::hash<void const volatile*>()(address) & 255;
}
extern "C" long atomic_monitor(void* address, int direct) {
  return direct ? std::__libcpp_atomic_monitor(static_cast<const volatile std::__cxx_atomic_contention_t*>(address))
                : std::__libcpp_atomic_monitor(address);
}
extern "C" void atomic_wait(void* address, long value, int direct) {
  if (direct) std::__libcpp_atomic_wait(static_cast<const volatile std::__cxx_atomic_contention_t*>(address), value);
  else std::__libcpp_atomic_wait(address, value);
}
extern "C" void atomic_notify(void* address, int direct, int all) {
  if (direct) {
    auto p = static_cast<const volatile std::__cxx_atomic_contention_t*>(address);
    if (all) std::__cxx_atomic_notify_all(p); else std::__cxx_atomic_notify_one(p);
  } else {
    if (all) std::__cxx_atomic_notify_all(address); else std::__cxx_atomic_notify_one(address);
  }
}
