//remark:mem-initializers without subsequent compound-stmt
//options:-A;fn:--c++17 -A;fn

  struct B{
  };
  struct D:B{
    template <typename T> D(T param):B(param);
  };

