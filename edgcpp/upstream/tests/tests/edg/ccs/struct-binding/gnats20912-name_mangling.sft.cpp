//type:cp
//options_all:--c++20 --c_to_obj_option -w

auto glam = [](auto a){
  typedef decltype(a) TD;
  TD vec[1] = {a};
  thread_local auto [ sb ] = vec;
  return sb;
};

auto&& x = []{return []{return 47;};};
auto y = x();

struct A {
  struct B {
    decltype ( glam(y) ) z ;
  } b;
} a = {y};
