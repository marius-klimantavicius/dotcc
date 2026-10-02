//type:fp
//options:--c++20

namespace avoid_private_members
{
  using INT = int;

  template<typename T>
  struct C
  {
  private:
    template<typename U>
    struct X
    { };

    using Y = X<int>;

  public:
    template<typename U>
    struct B
    {
      struct type
      { };
    };

    using A = B<X<INT>>;
  };

  template<typename T>
  struct D
  { };

  D<C<int>::A::type> d;         // cp_gen_be shouldn't try to use
                                // C<int>::X<int> here
}
