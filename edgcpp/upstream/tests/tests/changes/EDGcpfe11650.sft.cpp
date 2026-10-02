//type:fn
//options_all:--c++11
//remark:[4.4] Arithmetic conversions on scoped enumeration values (C++0x mode)
// 4/21/11  [EDGcpfe/11650]
//
// Arithmetic conversions on scoped enumeration values (C++0x mode)
//
// In C++0x mode, the front end previously erroneously considered "the usual
// arithmetic conversions" when dealing with built-in operators applied to
// scoped enumeration values.
//
// Now, built-in arithmetic operators involving a scoped enumeration value produce
// an error, and comparison operators (equality and relational) involving a scoped
// enumeration value are accepted only if both operands have the same type.
enum class E { e, f };
void g() {
  E::e+E::f;  // Previously accepted in C++0x mode; now an error.
}
