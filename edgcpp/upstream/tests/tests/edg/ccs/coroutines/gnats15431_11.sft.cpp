//type:fp
//options_all:--microsoft_version 1900 --set_flag coroutines -tused

#include <coroutine>
template <typename T>
std::generator<int> f(T) {
  co_yield 1;
}
void g() {
  f(1);
}
