//type:fn
//options_all:--c++20

module MOD:Part; // Not an interface unit!
export {

namespace N {
  void func_in_n();
  template<typename T> T templ_func_in_n(T x);
  struct S {
    int mem;
  };
  int var;
}

int global;
void global_func();

}
