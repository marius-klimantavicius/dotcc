//type:fn
//options_all: --c++11 --g++
struct Abstract { virtual void f() = 0; };

template<typename T> void foo(T) { };

int main() {
  foo<Abstract>(0);
}
