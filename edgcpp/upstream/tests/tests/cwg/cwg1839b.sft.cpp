//type:fn
//options_all:--c++20 -tused -A
namespace X {
  void p() {
    q(); // error: q not yet declared
    extern void q(); // q is a member of namespace X
    extern void r(); // r is a member of namespace X
  }
  }
  void q() { /* ... */ } // definition of X::q
}
