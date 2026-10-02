//type:fp
//options:--c++11:--c++20:--c++20 --gn 140100:--c++20 --clang_version 180100:--ms_c++20 --microsoft_version 1936
//options_all:-w -tused

namespace minimal
{
  template<typename T> struct X {
    ~X() { sizeof(T); }
  };
  struct Undefined;
  struct S {
    X<Undefined> m = {};  // Previously an error.  Now okay.
  };
}

namespace default_member_init
{
  template<typename T = void>
  struct B
  {
    ~B() { T::invalid; }
  };

  struct C
  {
    B<> b1{ };
    B<> b2 = { };
  };
}

namespace member_ctor
{
  template<typename T = void>
  struct B
  {
    B();
    B(int);
    ~B() { T::invalid; }
  };

  struct C
  {
    B<> b1{ };

#if defined(__clang__) || !defined(__GNUC__)
    // GCC triggers instantiation of the destructor in these cases
    B<> b2 = { };
    B<> b3{ 1 };
    B<> b4 = { 1 };
    B<> b5 = 1;
#endif
  };
}

namespace pr_example
{
  template<typename T>
  struct Wrapper
  {
  public:
    ~Wrapper() {
      T* p;
      p->foo();
    }
  };

  struct C
  {
    Wrapper<class D> d = {};
  };
}
