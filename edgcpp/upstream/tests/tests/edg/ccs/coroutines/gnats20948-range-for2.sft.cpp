//type:fp
//options::-DNEG;fn
//options_all:--c++20 --set_flag coroutines -tused

#include <coroutine>
using namespace std;

struct A {
  A& begin();
  A& end();
  bool operator!=(const A&);
  A& operator++();
  A& operator*();
#ifndef NEG
  bool await_ready();
  void await_suspend(coroutine_handle<>);
  A& await_resume();
#endif /* NEG */
};

task<int> g() {
  for co_await (auto i: A()) {}
  co_return 1;
}
