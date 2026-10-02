//type:fn
//options_all:--c++20

export module MOD;

struct S {
  export void mem_func();
  export int mem_var;
  export template<typename T> T templ_func(T x);
};
