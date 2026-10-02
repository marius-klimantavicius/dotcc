//type:fn
//options_all:--c++20

export module MOD;

export {
  struct S {
    void mem_func();
    int mem_var;
    template<typename T> T templ_func(T x);
  };

  export static_assert(sizeof(S) == sizeof(int));
}
