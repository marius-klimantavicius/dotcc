//type:fn
//options_all:--c++20

module;
export void glbl_fgmt_func(); // Cannot export from the global module fragment

export module MOD;
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

module : private;
export void pvt_fgmt_func(); // Cannot export from the private module fragment
