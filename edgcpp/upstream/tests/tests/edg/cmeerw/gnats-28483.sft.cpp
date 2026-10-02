//type:fp
//options:--c++17 --gn 150100:--c++17 --clang_version 210100

namespace minimal
{
  using type = __add_lvalue_reference(const int &);
  using type = const int &;
}

void f()
{
  {
    using type = __add_lvalue_reference(int);
    using type = int &;
  }

  {
    using type = __add_rvalue_reference(int);
    using type = int &&;
  }

  {
    using type = __add_lvalue_reference(const int);
    using type = const int &;
  }

  {
    using type = __add_rvalue_reference(const int);
    using type = const int &&;
  }

  {
    using type = __add_lvalue_reference(int &);
    using type = int &;
  }

  {
    using type = __add_rvalue_reference(int &);
    using type = int &;
  }

  {
    using type = __add_lvalue_reference(int &&);
    using type = int &;
  }

  {
    using type = __add_rvalue_reference(int &&);
    using type = int &&;
  }

  {
    using type = __add_lvalue_reference(const int &);
    using type = const int &;
  }

  {
    using type = __add_rvalue_reference(const int &);
    using type = const int &;
  }

  {
    using type = __add_lvalue_reference(const int &&);
    using type = const int &;
  }

  {
    using type = __add_rvalue_reference(const int &&);
    using type = const int &&;
  }
}
