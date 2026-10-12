using size_type = decltype(sizeof(0));
void *operator new(size_type, void *storage) noexcept { return storage; }
void operator delete(void *, void *) noexcept {}
struct alignas(64) Allocated {
  int value;
  Allocated(int value) : value(value) {}
  ~Allocated() {}
};
int *array_allocation(int count) { return new int[count]{}; }
void array_deallocation(int *value) { delete[] value; }
Allocated *aligned_allocation() { return new Allocated(42); }
Allocated *placement_allocation(void *storage) { return new(storage) Allocated{7}; }

namespace std {
#pragma define_type_info
class type_info {
public:
  virtual ~type_info();
  const char *name() const noexcept;
};
}
struct Polymorphic { virtual ~Polymorphic() {} };
const std::type_info &dynamic_type(Polymorphic &value) { return typeid(value); }
const std::type_info &static_type() { return typeid(int); }

int default_value() { return 11; }
int default_parameter(int value = default_value()) { return value; }
int lambda_uses(int value) {
  auto first = [value, initialized = default_parameter(3)](int extra) mutable constexpr {
    return value + initialized + extra;
  };
  auto second = [&first](auto extra) { return first(extra); };
  auto third = [=] { return value; };
  return second(1) + third() + default_parameter();
}

int initialization_effect_count;
int initialization_effect() { return ++initialization_effect_count; }
struct InitializationValue {
  int value;
  InitializationValue(int value) : value(value) {}
  InitializationValue(InitializationValue const &other)
    : value(other.value + initialization_effect()) {}
  operator bool() const { return value != 0; }
};
int source_initializations() {
  try {
    if (InitializationValue first(initialization_effect());
        InitializationValue condition = initialization_effect())
      throw InitializationValue(initialization_effect());
  } catch (InitializationValue caught) {
    return caught.value;
  }
  return 0;
}

namespace std {
template<class Result, class... Arguments> struct coroutine_traits {
  using promise_type = typename Result::promise_type;
};
template<class Promise = void> struct coroutine_handle;
template<> struct coroutine_handle<void> {
  void *pointer;
  static coroutine_handle from_address(void *address) noexcept { return {address}; }
  void *address() const noexcept { return pointer; }
};
template<class Promise> struct coroutine_handle {
  void *pointer;
  static coroutine_handle from_address(void *address) noexcept { return {address}; }
  void *address() const noexcept { return pointer; }
  operator coroutine_handle<void>() const noexcept { return {pointer}; }
};
}
struct ProtocolAwaiter {
  bool await_ready() const noexcept { return false; }
  void await_suspend(std::coroutine_handle<>) const noexcept {}
  int await_resume() const noexcept { return 7; }
};
struct ProtocolTask {
  struct promise_type {
    ProtocolTask get_return_object() { return {}; }
    ProtocolAwaiter initial_suspend() { return {}; }
    ProtocolAwaiter final_suspend() noexcept { return {}; }
    ProtocolAwaiter yield_value(int) noexcept { return {}; }
    void return_void() noexcept {}
    void unhandled_exception() {}
  };
};
ProtocolTask coroutine_protocol() {
  int value = co_await ProtocolAwaiter{};
  co_yield value;
  co_return;
}

struct CopyParameter {
  int value;
  CopyParameter(int value) : value(value) {}
  CopyParameter(CopyParameter const &other) : value(other.value) {}
  ~CopyParameter() {}
};
ProtocolTask coroutine_parameter(CopyParameter input) {
  co_await ProtocolAwaiter{};
  co_yield input.value;
  co_return;
}
struct MemberCoroutine {
  int value;
  ProtocolTask run(CopyParameter input) {
    co_await ProtocolAwaiter{};
    co_yield value + input.value;
    co_return;
  }
};

#if defined(__cpp_explicit_this_parameter)
struct ExplicitObject {
  int value;
  int read(this ExplicitObject const &self) { return self.value; }
};
int explicit_object(ExplicitObject const &object) { return object.read(); }
#endif

#if defined(__cpp_impl_reflection)
constexpr auto reflected_type = ^^Allocated;
auto reflected_tokens = ^^{ int interpolated = \[val, 42]; };
#endif
