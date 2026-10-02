//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cn:;cn

extern void ieq(int,int);

namespace M {
  struct F;
}
namespace N {
  class G;
  class X {
    friend struct M::F;
    class H;
    class Y {
      friend class N::G;
      friend class X::H;
      Y(int ii) : i(3 * ii - 1) { }
      int i;
    };
    int i;
    X(int ii) : i(ii) { }
  public:
    operator int() const { return i; }
  };
  class X::H {
  public:
    static void h(const X *px) {
      Y *py = new Y(*px);
      ieq(py->i, 92);
    }
  };
  class G {
  public:
    static void g(const X *px) {
      X::Y *py = new X::Y(*px);
      ieq(py->i, 92);
    }
  };
}
struct M::F {
  static void f() {
    N::X *px = new N::X(31);
    N::G::g(px);
    N::X::H::h(px);
  }
};

