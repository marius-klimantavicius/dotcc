//type:fp
//options_all:--c++20
//remark:[6.6] CTAD failure involving parameter pack expansion in alias template
// 8/2/23   [EDGcpfe/26299]
//
// CTAD failure involving parameter pack expansion in alias template
//
// In some fairly complex cases where an implicit deduction guide involves a
// parameter pack expansion in an alias template, class template argument
// deduction (CTAD) could fail when the class template was first declared with a
// forward declaration.
template<typename...> struct B {
  using type = int;
};
template<typename... TT> struct C;
template<typename... TT> struct C {
  template<typename> using A = typename B<TT ...>::type;
  template<A<int> = 0> C(TT ...);
};
C c{1};  // Previously a spurious error.  Now okay.
