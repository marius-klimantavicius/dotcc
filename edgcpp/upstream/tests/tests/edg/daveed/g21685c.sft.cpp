//remark:mem-initializers without subsequent compound-stmt
//options:-A;fn:--c++17 -A;fn

  struct S {
    int i;
    template<typename T> S(T i) : i(i);  // Previously aborted.  Now an error.
  };
