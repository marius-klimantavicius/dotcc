//type:fn
//options:--c++20:--ms_c++20

enum E
{
  E1
};

namespace namespace_scope
{
  static union
  {                             // error
    using E::E1;
  };
}

namespace fn_scope
{
  void foo()
  {
    union
    {                           // error
      using E::E1;
    };
  }
}

namespace class_scope
{
  struct C
  {
    union
    {                           // error
      using E::E1;
    };
  };
}
