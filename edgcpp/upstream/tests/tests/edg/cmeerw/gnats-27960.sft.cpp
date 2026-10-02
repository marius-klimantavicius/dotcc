//type:fp
//options:--c++11:--c++20 -DMBR_PTR:--c++20 --gn 140200 -DMBR_PTR:--c++20 --clang_version 190100 -DMBR_PTR:--ms_c++20 --microsoft_version 1942 -DMBR_PTR
//options_all:-w -tused

namespace minimal
{
  using P = struct { int i; };
  void f(P *p) {
    p->~P();
  }
}

namespace alternative
{
  template <typename a> void b(a c) { c.~a(); }
  typedef struct { } d;
  struct e {
    d f;
  };
  void g(e *h) {
    b(h->f);
  }
}

#ifdef MBR_PTR
namespace mbr_ptr
{
  class C {
  public:
    void publicFunc();

  protected:
    void protectedFunc();
  };

  template<typename T, void (C::*p)()>
  struct B
  { };

  template<typename T> struct B<T, &C::publicFunc>
  { };

  template<typename T> struct B<T, &C::protectedFunc>
  { };
}
#endif
