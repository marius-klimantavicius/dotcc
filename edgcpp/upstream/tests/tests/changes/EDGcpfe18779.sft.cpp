//type:fp
//options_all:--c++14
//remark:[5.0] Core issue 1601: Enum promotion rules
// 10/17/17 [EDGcpfe/18779]
//
// Core issue 1601: Enum promotion rules
//
// The front end now implements core issue 1601, which specifies that during
// overload resolution promotion of an unscoped enum type to its explicitly-
// specified underlying type is a better match than other promotions.  This change
// takes effect in C++14, but not in Clang and GCC modes (since those compilers do
// not appear to implement the new rules yet).
enum E: char { e };
void f(char);
void f(int) = delete;
void g() {
  f(e);      // Previously ambiguous.  Now okay in C++14 mode.
}            // (The call selects the non-deleted function f.)
