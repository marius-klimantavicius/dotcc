//type:cp
//options_all:--c++20 --set_flag=coroutines --set_flag=no_checking_pragmas -tused
//linker_options:--c_to_obj_option -Dco_return=return
//require:BACK_END_IS_CP_GEN_BE 1

#include <coroutine>
using namespace std;

struct A {
  struct promise_type {
    promise_type();
    ~promise_type();
    auto initial_suspend() { return suspend_always{}; }
    auto final_suspend() noexcept { return suspend_always{}; }
    void unhandled_exception();
    A get_return_object();
    void return_value(const A&);
  };
} a;

struct B {
  B();
  B(const B&);
  B(B&&);

  B& operator=(const B&);
private:
  int a;
  float& b;
  B&& c;
};

A f(int i, float& f, B b, B* pb, B& rb, B&& rrb) {
  i = 0;
  f = 0.0f;
  b = *pb;
  rb = rrb;
  co_return a;
}

struct C {
  A f(double d, char c, C carr[], C carr2[10]) {
    d = 0.0;
    c = '0';
    carr[0].x = 0;
    carr2[5].x = 0;
    co_return a;
  }
  int x;
};
