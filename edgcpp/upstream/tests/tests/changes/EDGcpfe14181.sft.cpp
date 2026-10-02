//type:fp
//options_all:--c++14
//remark:[4.10] C++14: Value category of member selections (IL CHANGE)
// 11/26/14 [EDGcpfe/14181,EDGcpfe/15367]
//
// C++14: Value category of member selections (IL CHANGE)
//
// The C++14 standard made a significant change to the value category of an
// expression of the form prvalue.m or prvalue.*pm: The result is now an xvalue
// (this change came about by the resolution of the C++ standardization
// committee's Core issue 616; in C++11 it is a prvalue).  The front end now
// implements that rule (except that in GNU or Clang C++14 modes, the C++11 rule
// is retained for compatibility with the GCC and Clang compilers).
//
// Note that this is a subtle IL CHANGE.
template<typename T, typename U> struct is_same {
  static bool const value = false;
};
template <typename T> struct is_same<T, T> {
  static bool const value = true;
};
struct X {};
struct S { X m; } s;
static_assert(is_same<decltype((S().m)), X&&>::value, "Not C++14");
  // Now accepted in C++14 mode.
