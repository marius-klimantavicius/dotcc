//type:fp
//options_all:--c++20
//remark:[6.5] Correspondence of declarations with trailing requires clause
// 4/12/23  [EDGcpfe/26119]
//
// Correspondence of declarations with trailing requires clause
//
// A using declaration in class scope only adds those declarations from a base
// class that do not correspond to (and thus would conflict with) other
// declarations in the class.  However, the front end previously did not take
// trailing requires clauses into account when checking whether declarations
// correspond.
struct B { };
template<typename T> struct D : B {
  D() requires (sizeof(T) > sizeof(char));
  using B::B;  // B's default constructor was not inherited.
};
D<char> d;     // Previously a spurious error.  Now okay.
