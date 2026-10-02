//type:fn
//options_all:--c++20 -tused -A
namespace X {
  void p() {
    extern void q(); // q is a member of namespace X
    extern void r(); // r is a member of namespace X
  }
  void q() { /* ... */ } // definition of X::q
}
void q() { /* ... */ } // some other, unrelated q
void X::r() { /* ... */ } // error: r cannot be declared by qualified-id
