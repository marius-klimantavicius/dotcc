//type:fp
//options_all:--g++ --c++11
//remark:[5.0] Generic casts: tpck_cast and tpck_expression (IL CHANGE)
// 11/1/17  [EDGcpfe/17707,EDGcpfe/18676]
//
// Generic casts: tpck_cast and tpck_expression (IL CHANGE)
//
// The front end previously generated both tpck_cast and tpck_expression entries
// for generic casts in contexts that may require a constant-expression.  In some
// cases, this distinction led to spurious mismatches that in turn produced
// spurious errors.
//
// Previously, the front end was unable to distinguish the two partial
// specializations; now it picks the second one.  The resolution of this issue
// required the elimination of the tpck_cast variant of ck_template_param
// constants, which is an IL CHANGE (such entries are now represented instead by
// tpck_expression entries pointing to a cast expression).
template<bool> struct X;
template<typename T> struct V {
  static const int value = 1;
};
template<typename T, typename = void> struct R;
template<typename T, bool cond> struct R<T, X<cond>> {};
template<typename T> struct R<T, X<V<T>::value>> {};
R<int, X<1>> r;  // Previously an error.
