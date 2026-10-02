//type:cp
//options_all:--c++14

struct A {
  int x = [=](auto a) { [this]{}; return 0; }(0);
};
