//===----------------------------------------------------------------------===//
//
// Part of the LLVM Project, under the Apache License v2.0 with LLVM Exceptions.
// See https://llvm.org/LICENSE.txt for license information.
// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
//
//===----------------------------------------------------------------------===//

#include <__atomic/contention_t.h>
#include <__functional/hash.h>

// Managed port: retain libc++'s contention table, sequence laundering and
// memory orders. Replace only the OS wait/wake boundary with BCL monitors.
// Portable target __cxx_contention_t is int64_t, independent of the host OS.
extern "C" void __edg_libcpp_atomic_wait64(const volatile void*, long long) noexcept;
extern "C" void __edg_libcpp_atomic_wake64(const volatile void*) noexcept;
extern "C" void* __edg_libcpp_atomic_table() noexcept;

_LIBCPP_BEGIN_NAMESPACE_STD
static_assert(sizeof(__cxx_contention_t) == 8 && alignof(__cxx_atomic_contention_t) >= 8,
              "managed atomic wait requires aligned 64-bit contention storage");
static void __libcpp_platform_wait_on_address(
    __cxx_atomic_contention_t const volatile* p, __cxx_contention_t value) {
  __edg_libcpp_atomic_wait64(p, static_cast<long long>(value));
}
static void __libcpp_platform_wake_by_address(
    __cxx_atomic_contention_t const volatile* p, bool) {
  // Hash buckets can contain different addresses. Wake all so a notify_one
  // never wakes only a waiter for an unrelated address and loses progress.
  __edg_libcpp_atomic_wake64(p);
}

static constexpr size_t __libcpp_contention_table_size = (1 << 8); /* < there's no magic in this number */

struct alignas(64) /*  aim to avoid false sharing */ __libcpp_contention_table_entry {
  __cxx_atomic_contention_t __contention_state;
  __cxx_atomic_contention_t __platform_state;
  inline constexpr __libcpp_contention_table_entry() : __contention_state(0), __platform_state(0) {}
};

static_assert(sizeof(__libcpp_contention_table_entry) == 64 &&
              alignof(__libcpp_contention_table_entry) == 64 &&
              __builtin_offsetof(__libcpp_contention_table_entry, __contention_state) == 0 &&
              __builtin_offsetof(__libcpp_contention_table_entry, __platform_state) == 8,
              "managed shared contention table layout");

static hash<void const volatile*> __libcpp_contention_hasher;

static __libcpp_contention_table_entry* __libcpp_contention_state(void const volatile* p) {
  // Synchronization for an explicitly shared atomic must cross engine owners.
  // Only this private runtime table uses program storage; user globals retain
  // the CLI-selected instance behavior. Zeroed pinned bytes match the original
  // constexpr constructor of both contention words.
  auto table = static_cast<__libcpp_contention_table_entry*>(__edg_libcpp_atomic_table());
  return &table[__libcpp_contention_hasher(p) & (__libcpp_contention_table_size - 1)];
}

/* Given an atomic to track contention and an atomic to actually wait on, which may be
   the same atomic, we try to detect contention to avoid spuriously calling the platform. */

static void __libcpp_contention_notify(__cxx_atomic_contention_t volatile* __contention_state,
                                       __cxx_atomic_contention_t const volatile* __platform_state,
                                       bool __notify_one) {
  if (0 != __cxx_atomic_load(__contention_state, memory_order_seq_cst))
    // We only call 'wake' if we consumed a contention bit here.
    __libcpp_platform_wake_by_address(__platform_state, __notify_one);
}
static __cxx_contention_t
__libcpp_contention_monitor_for_wait(__cxx_atomic_contention_t volatile* /*__contention_state*/,
                                     __cxx_atomic_contention_t const volatile* __platform_state) {
  // We will monitor this value.
  return __cxx_atomic_load(__platform_state, memory_order_acquire);
}
static void __libcpp_contention_wait(__cxx_atomic_contention_t volatile* __contention_state,
                                     __cxx_atomic_contention_t const volatile* __platform_state,
                                     __cxx_contention_t __old_value) {
  __cxx_atomic_fetch_add(__contention_state, __cxx_contention_t(1), memory_order_seq_cst);
  // We sleep as long as the monitored value hasn't changed.
  __libcpp_platform_wait_on_address(__platform_state, __old_value);
  __cxx_atomic_fetch_sub(__contention_state, __cxx_contention_t(1), memory_order_release);
}

/* When the incoming atomic is the wrong size for the platform wait size, need to
   launder the value sequence through an atomic from our table. */

static void __libcpp_atomic_notify(void const volatile* __location) {
  auto const __entry = __libcpp_contention_state(__location);
  // The value sequence laundering happens on the next line below.
  __cxx_atomic_fetch_add(&__entry->__platform_state, __cxx_contention_t(1), memory_order_release);
  __libcpp_contention_notify(
      &__entry->__contention_state,
      &__entry->__platform_state,
      false /* when laundering, we can't handle notify_one */);
}
_LIBCPP_EXPORTED_FROM_ABI void __cxx_atomic_notify_one(void const volatile* __location) {
  __libcpp_atomic_notify(__location);
}
_LIBCPP_EXPORTED_FROM_ABI void __cxx_atomic_notify_all(void const volatile* __location) {
  __libcpp_atomic_notify(__location);
}
_LIBCPP_EXPORTED_FROM_ABI __cxx_contention_t __libcpp_atomic_monitor(void const volatile* __location) {
  auto const __entry = __libcpp_contention_state(__location);
  return __libcpp_contention_monitor_for_wait(&__entry->__contention_state, &__entry->__platform_state);
}
_LIBCPP_EXPORTED_FROM_ABI void __libcpp_atomic_wait(void const volatile* __location, __cxx_contention_t __old_value) {
  auto const __entry = __libcpp_contention_state(__location);
  __libcpp_contention_wait(&__entry->__contention_state, &__entry->__platform_state, __old_value);
}

/* When the incoming atomic happens to be the platform wait size, we still need to use the
   table for the contention detection, but we can use the atomic directly for the wait. */

_LIBCPP_EXPORTED_FROM_ABI void __cxx_atomic_notify_one(__cxx_atomic_contention_t const volatile* __location) {
  __libcpp_contention_notify(&__libcpp_contention_state(__location)->__contention_state, __location, true);
}
_LIBCPP_EXPORTED_FROM_ABI void __cxx_atomic_notify_all(__cxx_atomic_contention_t const volatile* __location) {
  __libcpp_contention_notify(&__libcpp_contention_state(__location)->__contention_state, __location, false);
}
_LIBCPP_EXPORTED_FROM_ABI __cxx_contention_t
__libcpp_atomic_monitor(__cxx_atomic_contention_t const volatile* __location) {
  return __libcpp_contention_monitor_for_wait(&__libcpp_contention_state(__location)->__contention_state, __location);
}
_LIBCPP_EXPORTED_FROM_ABI void
__libcpp_atomic_wait(__cxx_atomic_contention_t const volatile* __location, __cxx_contention_t __old_value) {
  __libcpp_contention_wait(&__libcpp_contention_state(__location)->__contention_state, __location, __old_value);
}

_LIBCPP_END_NAMESPACE_STD
