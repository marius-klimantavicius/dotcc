//remark:mem-initializers without subsequent compound-stmt
//options:-A;fn:--c++17 -A;fn

  struct Struct1 {
    int m_int;
    template < typename T > Struct1( T a ) : m_int(a);
  };

  //The following code also makes the frontend report an assertion error
  /*
  struct B{
  };
  struct D:B{
    template <typename T> D(T param):B(param);
  };
  */
