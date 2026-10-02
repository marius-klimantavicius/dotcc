//type:cp
//options::--no_exceptions
//options_all:--c++17 -tused

template<typename T>
void f() noexcept(true);

void g() {
  f<int>();
}
