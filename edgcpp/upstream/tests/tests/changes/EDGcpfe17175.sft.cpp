//type:fp
//options_all:--microsoft
//remark:[4.12] Microsoft compatibility: __is_convertible_to
// 5/24/16  [EDGcpfe/17175]
//
// Microsoft compatibility: __is_convertible_to
//
// The changes for EDGcpfe/16645 updated the implementation of the type traits
// helper __is_convertible_to to more accurately model std::is_convertible as
// specified in the standard.  Specifically, the standard specification describes
// the conversion being tested in terms of the conversions that occur when
// returning a value from a function.  Ordinarily, that is a kind of "copy
// initialization", but in Microsoft mode, returning a value is a form of "direct
// initialization" (see the entry of 1/18/98).  However, it appears that the
// Microsoft compiler does not treat the "simulated" return operation involved
// in the computation of __is_convertible_to as a direct initialization (even
// though it still treats actual return operations that way).
//
// Here the conversion function from F to T is explicit and therefore not
// applicable in a standard return value (explicit conversion functions are not
// considered for copy initialization).  MSVC does consider it in an actual
// return statement (because there it is treated as direct initialization), but
// not for the __is_convertible_to(F, T) evaluation.  The front end now matches
// MSVC in Microsoft mode.
struct F {
  template<typename T> explicit operator T() const;
};
struct T {};
static_assert(!__is_convertible_to(F, T), "Not like MSVC");
  // Previously failed in Microsoft mode; now okay.
