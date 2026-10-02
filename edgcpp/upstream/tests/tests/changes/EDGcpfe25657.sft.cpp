//type:fp
//options_all:--clang_v 130001
//remark:[6.5] Expression operands of typeid
// 5/17/23  [EDGcpfe/25657,EDGcpfe/26023,EDGcpfe/26101]
//
// Expression operands of typeid
//
// Previously, the expression operand of a typeid operator was parsed by default
// as an evaluated operand, even when it was not of a polymorphic type.  The front
// end did perform some compensations if it turned out that no evaluation is
// needed, but some side effects of the processing -- like the instantiation of
// used entities -- remained.  That could result in spurious errors.
//
// Previously, this triggered the instantiation of S<char>::f() -- even though the
// reference to that function is not evaluated -- and subsequently an error
// because "*px >> 2" is not valid during that instantiation.  Now the example is
// accepted because the front end processes the operand of typeid as a true
// unevaluated operand.
#include <typeinfo>
template<typename> struct X;
template<typename T> struct S {
  X<T> *px;
  S f() { *px >> 2; return *this; }
};
void g() {
  S<char> sc;
  (void)typeid(sc.f());
}
