//type:fn
//options_all:--c++17 -w --sys_include $RUN_TEST_CURR_DIR/include

#include <initializer_list>

template<typename T> struct X {
  X(std::initializer_list<T>) = delete;
  X(const X&);
};

template<typename T> struct Y;

template<typename T> struct Z {};

void bar(Y<int>& y, Z<int>& z) {
  X x1{y};
  X x2(y);
  X x3{z};
  X x4(z);
}

template<typename T> struct Y : X<int> {};

