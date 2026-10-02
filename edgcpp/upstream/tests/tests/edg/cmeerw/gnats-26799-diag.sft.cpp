//type:fn
//options:--c++20 --gn 130200:--c++20 --clang_version 160000

namespace declared_in_class
{
  template<typename>
  struct C {
    struct A;
    template<typename> friend struct A; // accepted by MSVC
  };
}

namespace declared_typedef_in_class
{
  template<typename>
  struct C {
    using A = int;
    template<typename> friend struct A; // error
  };
}

namespace declared_enum_in_class
{
  template<typename>
  struct C {
    enum A { };
    template<typename> friend struct A; // error
  };
}

namespace friend_lookup_using_contained
{
  namespace ns
  {
    namespace ns2
    {
      template<int>
      struct A;
    }

    using namespace ns2;

    template<typename>
    struct C
    {
      template<typename>
      friend struct A;          // conflicts
    };
  }

  template struct ns::C<int>;
}

namespace friend_in_ms_instantiated_nonreal_class
{
  template<class T> struct A
  {
    template<class U> friend struct C;
    C<T> member;                // accepted by MSVC
  };

  template<class T> struct B : A<T>
  { };

  template<class U> struct C { U member; };

  C<int> c;
}

namespace find_non_template_class
{
  template<int>
  struct D;

  template<int>
  struct C
  {
    struct D;

    struct B
    {
      template<int> friend struct D; // error, conflicts with C<int>::D, accepted by MSVC
    };
  };

  C<0>::B b;
}

namespace find_non_template_class_template_id
{
  template<typename T>
  struct D;

  template<int>
  struct C
  {
    struct D;

    struct B
    {
      template<typename T> friend struct D<T>;  // error, conflicts with C<int>::D
    };
  };
}

namespace template_parameter_found
{
  template<typename V, int W>
  struct A
  {
    friend W;                   // error

    friend V;                   // okay

    friend struct V;            // error

    template<int>
    friend struct V;            // error, conflicts with template parameter
  };

  A<int> a;
}
