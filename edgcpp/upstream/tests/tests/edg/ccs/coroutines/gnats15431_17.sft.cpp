//type:fp
//options_all:--microsoft_version 1900 --set_flag coroutines -tused

#include <coroutine>
std::generator<int> f(int arg0) {
  co_yield arg0;
  co_return arg0;
}
