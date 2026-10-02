//type:fp
//options_all:--c++17
//remark:[6.0] SFINAE failure with decltype of invalid code
// 7/30/19  [EDGcpfe/21435]
//
// SFINAE failure with decltype of invalid code
//
// When invalid code appeared in a decltype expression in a template argument,
// the front end may have incorrectly deduced the decltype of the expression as
// something other than an error type.  This could lead to SFINAE (or other)
// failures.
//
// This is now fixed.
struct AGGR {
  short x;
};
void func(AGGR);
template <typename, typename To, typename From>
struct Test {
  static constexpr bool value = false;
};
template <typename To, typename From>
struct Test<decltype( func({From()}) ), To, From> {
  static constexpr bool value = true;
};
/* Previously used the specialization of Test, causing the assert to fail.
   The expression func({From()}) in the partial specialization's template
   argument list attempts to initialize AGGR::x, of type short, from a value
   of type float.  This is a narrowing error, which should cause the partial
   specialization not to match the template arguments used in the
   static_assert condition. */
static_assert(!Test<void, AGGR, float>::value);
