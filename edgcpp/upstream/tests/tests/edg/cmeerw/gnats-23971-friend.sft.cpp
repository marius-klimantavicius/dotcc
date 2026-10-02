//type:fn
//options:--c++03:c++20
namespace cls
{
  struct C
  {
    template<typename T>
    void foo(T);

    template<>
    friend void foo(int)        // explicit specialization friend
    { }
  };
}

namespace non_mbr
{
  template<typename T>
  void foo(T);

  struct C
  {
    template<>
    friend void foo(int)        // explicit specialization friend
    { }
  };
}
