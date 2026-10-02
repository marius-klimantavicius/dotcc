//type:fp
//options:--c++11:--c++20:--c++20 --gn 130200:--c++20 --clang_version 160000

namespace friend_lookup
{
  template<int>
  struct A;

  namespace ns
  {
    template<typename>
    struct C
    {
      template<typename>
      friend struct A;          // conflicts for MSVC
    };
  }

  template struct ns::C<int>;
}

namespace friend_lookup_using_not_contained
{
  namespace ns2
  {
    template<int>
    struct A;
  }

  namespace ns
  {
    using namespace ns2;

    template<typename>
    struct C
    {
      template<typename>
      friend struct A;          // conflicts for MSVC
    };
  }

  template struct ns::C<int>;
}
