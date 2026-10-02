//type:fn
//options_all:--microsoft_version 1900 --set_flag coroutines -tused

#include <coroutine>
std::generator<int> f(...) {
  co_yield 0;
}
