//type:fp
//options::-DNEG;fn
//options_all:--c++20 --set_flag coroutines -tused

#include <coroutine>
using namespace std;

struct Future {
  bool await_ready();
  void await_suspend(coroutine_handle<>);
  int await_resume();
};

struct A {};

#ifndef NEG
Future operator co_await(A);
#endif

task<int> g() {
  int i = co_await A();
  co_return 1;
}
