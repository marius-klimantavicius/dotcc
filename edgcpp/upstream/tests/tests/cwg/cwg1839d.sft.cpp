//type:fn
//options_all:--c++20 -tused -A
namespace X {
  void p() {
    extern void q(); // q is a member of namespace X
    extern void r(); // r is a member of namespace X
  }
  void middle() {
    q(); // error: q not visible to name lookup
  }
  void q() { /* ... */ } // definition of X::q
}
