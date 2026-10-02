//type:cp
//options:--c++14;fn:--c++17:--c++17 --no_exceptions

void foo(void (*func)()) {}
void foo(void (*func)() noexcept) {}
