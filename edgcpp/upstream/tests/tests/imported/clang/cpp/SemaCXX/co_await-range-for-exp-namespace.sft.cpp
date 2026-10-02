//type: fn
//options:  --c++14
# 1 "SemaCXX/co_await-range-for-exp-namespace.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 397 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/co_await-range-for-exp-namespace.cpp" 2



# 1 "SemaCXX/Inputs/std-coroutine-exp-namespace.h" 1




namespace std {
namespace experimental {
template <class Ret, typename... T>
struct coroutine_traits { using promise_type = typename Ret::promise_type; };

template <class Promise = void>
struct coroutine_handle {
  static coroutine_handle from_address(void *) noexcept;
};
template <>
struct coroutine_handle<void> {
  template <class PromiseType>
  coroutine_handle(coroutine_handle<PromiseType>) noexcept;
  static coroutine_handle from_address(void *);
};

struct suspend_always {
  bool await_ready() noexcept { return false; }
  void await_suspend(coroutine_handle<>) noexcept {}
  void await_resume() noexcept {}
};

struct suspend_never {
  bool await_ready() noexcept { return true; }
  void await_suspend(coroutine_handle<>) noexcept {}
  void await_resume() noexcept {}
};
}
}
# 5 "SemaCXX/co_await-range-for-exp-namespace.cpp" 2

using namespace std::experimental;

template <class Begin>
struct Awaiter {
  bool await_ready();
  void await_suspend(coroutine_handle<>);
  Begin await_resume();
};

template <class Iter> struct BeginTag { BeginTag() = delete; };
template <class Iter> struct IncTag { IncTag() = delete; };

template <class Iter, bool Delete = false>
struct CoawaitTag { CoawaitTag() = delete; };

template <class T>
struct Iter {
  using value_type = T;
  using reference = T &;
  using pointer = T *;

  IncTag<Iter> operator++();
  reference operator*();
  pointer operator->();
};
template <class T> bool operator==(Iter<T>, Iter<T>);
template <class T> bool operator!=(Iter<T>, Iter<T>);

template <class T>
struct Range {
  BeginTag<Iter<T>> begin();
  Iter<T> end();
};

struct MyForLoopArrayAwaiter {
  struct promise_type {
    MyForLoopArrayAwaiter get_return_object() { return {}; }
    void return_void();
    void unhandled_exception();
    suspend_never initial_suspend();
    suspend_never final_suspend() noexcept;
    template <class T>
    Awaiter<T *> await_transform(T *) = delete;
  };
};
MyForLoopArrayAwaiter g() {
  int arr[10] = {0};
  for co_await (auto i : arr) {}



}

struct ForLoopAwaiterBadBeginTransform {
  struct promise_type {
    ForLoopAwaiterBadBeginTransform get_return_object();
    void return_void();
    void unhandled_exception();
    suspend_never initial_suspend();
    suspend_never final_suspend() noexcept;

    template <class T>
    Awaiter<T> await_transform(BeginTag<T>) = delete;

    template <class T>
    CoawaitTag<T> await_transform(IncTag<T>);
  };
};
ForLoopAwaiterBadBeginTransform bad_begin() {
  Range<int> R;
  for co_await (auto i : R) {}


}
template <class Dummy>
ForLoopAwaiterBadBeginTransform bad_begin_template(Dummy) {
  Range<Dummy> R;
  for co_await (auto i : R) {}


}
template ForLoopAwaiterBadBeginTransform bad_begin_template(int);

template <class Iter>
Awaiter<Iter> operator co_await(CoawaitTag<Iter, true>) = delete;


struct ForLoopAwaiterBadIncTransform {
  struct promise_type {
    ForLoopAwaiterBadIncTransform get_return_object();
    void return_void();
    void unhandled_exception();
    suspend_never initial_suspend();
    suspend_never final_suspend() noexcept;

    template <class T>
    Awaiter<T> await_transform(BeginTag<T> e);

    template <class T>
    CoawaitTag<T, true> await_transform(IncTag<T>);
  };
};
ForLoopAwaiterBadIncTransform bad_inc_transform() {
  Range<float> R;
  for co_await (auto i : R) {}


}

template <class Dummy>
ForLoopAwaiterBadIncTransform bad_inc_transform_template(Dummy) {
  Range<Dummy> R;
  for co_await (auto i : R) {}


}
template ForLoopAwaiterBadIncTransform bad_inc_transform_template(long);



template <class T>
constexpr void never_instant(T) {
  static_assert(sizeof(T) != sizeof(T), "function should not be instantiated");
  for co_await (auto i : foo(T{})) {}

}

namespace NS {
struct ForLoopAwaiterCoawaitLookup {
  struct promise_type {
    ForLoopAwaiterCoawaitLookup get_return_object();
    void return_void();
    void unhandled_exception();
    suspend_never initial_suspend();
    suspend_never final_suspend() noexcept;
    template <class T>
    CoawaitTag<T, false> await_transform(BeginTag<T> e);
    template <class T>
    Awaiter<T> await_transform(IncTag<T>);
  };
};
}
using NS::ForLoopAwaiterCoawaitLookup;

template <class T>
ForLoopAwaiterCoawaitLookup test_coawait_lookup(T) {
  Range<T> R;
  for co_await (auto i : R) {}

}
template ForLoopAwaiterCoawaitLookup test_coawait_lookup(int);



namespace NS2 {
template <class Iter>
Awaiter<Iter> operator co_await(CoawaitTag<Iter, false>);
}
using NS2::operator co_await;
template ForLoopAwaiterCoawaitLookup test_coawait_lookup(long);
