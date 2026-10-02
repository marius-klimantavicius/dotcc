//type:fp
//options_all:--c++20
//remark:[6.5] Abort on CTAD with constrained template parameter
// 2/15/23  [EDGcpfe/26050]
//
// Abort on CTAD with constrained template parameter
//
// When forming an implicit deduction guide for a class template declared with a
// constrained template parameter, the type constraint was not correctly
// substituted.  If the concept was declared with a variadic template parameter,
// this would result in an abort in check_type_constraint.
// --c++20:
template<typename T, typename ... U> concept C = true;
template<C<int> T> struct D {
  D(T) { };
};
D d{1};  // Previously aborted.  Now deduced as D<int>.
