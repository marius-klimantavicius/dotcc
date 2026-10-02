//type:fn
//options:--microsoft_version 1900
//options_all:--set_flag coroutines -tused

#include <coroutine>
class S {
S() {
  co_yield 1;
  co_return;
}
};

class C {
~C() {
  co_await 1;
  co_return;
}
};
