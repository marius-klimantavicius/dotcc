//type:fp
//options:--ms_c++20 --microsoft_version 1936

namespace declared_in_class
{
  template<typename>
  struct C {
    struct A;
    template<typename> friend struct A; // accepted by MSVC
  };
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
