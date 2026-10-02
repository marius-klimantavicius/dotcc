//type:fp
//options_all:--gnu_version 40805 --c++11
//remark:[5.1] Matching implicit conversion templates
// 9/11/18  [EDGcpfe/19550]
//
// Matching implicit conversion templates
//
// In general, when performing an implicit conversion that involves a user-defined
// conversion function, the invocation of that user-defined conversion function
// can be followed by another standard conversion (e.g., an integral promotion).
// However, the C++ standard specifies that that subsequent standard conversion
// must be trivial (an "exact match") if the user-defined conversion is a template
// instance.  Previously, the front end did not implement that last rule; now, it
// does.
template<typename T> struct Id { typedef T Type; };
struct S {
  template<typename T = short> operator typename Id<T>::Type();
};
static_assert(!__is_convertible_to(S,int), "Unexpected");
  // Previously this assertion failed.  Now it passes.
