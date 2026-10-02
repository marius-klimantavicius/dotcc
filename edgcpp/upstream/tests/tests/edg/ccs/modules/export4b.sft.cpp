//type:cp
//options::-DNEG;fn
//options_all:--c++20

export module MOD;

#if NEG
export {
#endif
  export {
    namespace N {
      export {
	void func_in_n();
	template<typename T> T templ_func_in_n(T x);
      }
      export struct S {
	int mem;
      };
      int var;
    }
    int global;
    void global_func();
  }
#if NEG
}
#endif
