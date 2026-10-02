//type:fp
//options:--c++11

// DELETE_CAN_BE_FOLDED_INTO_DTOR and LOWERING_REMOVES_UNNEEDED_CONSTRUCTIONS_AND_DESTRUCTIONS
// !IA64_ABI

namespace minimal
{
  struct B {
    ~B() noexcept(false) { }
  };
  using C = B;
  void f(C *c) {
    delete c;
  }
}

namespace no_exception_spec
{
  struct B
  {
    ~B()
    { }
  };

  using C = B;
  void f(C *c)
  {
    delete c;
  }
}

namespace noexcept_true
{
  struct B
  {
    ~B() noexcept
    { }
  };

  using C = B;
  void f(C *c)
  {
    delete c;
  }
}

namespace noexcept_false
{
  struct B
  {
    ~B() noexcept(false)
    { }
  };

  using C = B;
  void f(C *c)
  {
    delete c;
  }
}
