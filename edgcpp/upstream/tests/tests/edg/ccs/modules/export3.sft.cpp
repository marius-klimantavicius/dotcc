//type:cp
//options_all:--c++20

export module MOD;

namespace N {
  export void func_in_n();
  export template<typename T> T templ_func_in_n(T x);
  export struct S {
    int mem;
  };
  export int var;
}
