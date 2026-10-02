//type:fp
//options_all:--c++20
//remark:[6.5] Core issue 2664: Deduction failure in CTAD for alias templates
// 2/13/23  [EDGcpfe/25901]
//
// Core issue 2664: Deduction failure in CTAD for alias templates
//
// The resolution of Core issue 2664 clarified that when forming deduction guides
// for an alias template, a deduction failure for the return type of the deduction
// guide is treated as deducing an empty set of template arguments.
// with --c++20:
template<typename T> struct C {
  template<typename U> C(U);
};
template<typename T> C(T) -> C<T *>;
template<typename T> using A = C<T>;
A a{1};  // Previously a deduction failure.  Now deduced as A<int *>.
