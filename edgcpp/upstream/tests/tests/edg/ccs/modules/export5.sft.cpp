//type:cp
//options_all:--c++20

export module MOD;

namespace N {
  void func_in_n();
  template<typename T> T templ_func_in_n(T x);
  struct S {
    int mem;
  };
  int var;
}

export using N::func_in_n;
export using N::templ_func_in_n;
export using N::S;
export using N::var;
export extern int global_var;
export void global_func();

int global_var = 10;
void global_func() {}
