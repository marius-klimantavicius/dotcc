//type:fp
//options_all:--c++11
//remark:[4.11] Segfault in get_substitution_pairs_for_template_class
// 9/23/15  [EDGcpfe/16533]
//
// Segfault in get_substitution_pairs_for_template_class
//
// A segfault had occurred in cases where a template parameter is used as
// an argument to an attribute (or attribute-like entity) in a member of
// an explicit specialization.  Now fixed.
template<typename> struct A;
template<> struct A<void> {
  template<int N> struct alignas(N) B {};
};
A<void>::B<1> a;
