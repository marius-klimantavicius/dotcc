//type:fn
//options:--microsoft_version 1900
//options_all:--set_flag coroutines -tused

#include <coroutine>
void f(int x = co_yield 5);
