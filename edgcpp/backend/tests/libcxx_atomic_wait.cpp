extern "C" void __edg_libcpp_atomic_wait64(const volatile void*, long long) noexcept;
extern "C" void __edg_libcpp_atomic_wake64(const volatile void*) noexcept;
static long word;
extern "C" long* wait_word() { return &word; }
extern "C" void wait_value(long* p, long value) { __edg_libcpp_atomic_wait64(p, value); }
extern "C" void publish_value(long* p, long value) {
  __atomic_store_n(p, value, __ATOMIC_SEQ_CST);
  __edg_libcpp_atomic_wake64(p);
}
