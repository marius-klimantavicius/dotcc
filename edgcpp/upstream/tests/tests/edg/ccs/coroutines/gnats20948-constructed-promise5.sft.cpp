//type:fn
//options_all:--c++20 -tused --set_flag coroutines -tused

#include <coroutine>
using namespace std;

struct B;
template<typename T>
struct C;

struct A {
  struct promise_type {
    promise_type(B*, int, float);
    promise_type(B*, const A&);
    template<typename T>
    promise_type(C<T>*, double);
    template<typename T>
    promise_type(C<T>*, const A&, const A&);
    promise_type() = delete;
    ~promise_type();
    auto initial_suspend() { return suspend_always{}; }
    auto final_suspend() noexcept { return suspend_always{}; }
    void unhandled_exception();
    A get_return_object();
    void return_void();
  };
};

struct B {
  template<class T>
  T f(int i, float f) {
    co_return;
  }

  template<class T>
  T g(double) {
    co_return;
  }

  template<class T>
  A h(T) {
    co_return;
  }
};

template<class T>
struct C {
  T f(int, float) {
    co_return;
  }
  
  T g(double) {
    co_return;
  }

  A h(T, T) {
    co_return;
  }
};

void foo() {
  B b;
  C<A> c;
  C<int> ci;
  
  b.f<A>(0, 1.0f);
  b.g<A>(1.0);
  b.h(A());
  b.h(1);

  c.f(0, 1.0f);
  c.g(1.0);
  c.h(A(), A());
  ci.h(1, 1);
}
