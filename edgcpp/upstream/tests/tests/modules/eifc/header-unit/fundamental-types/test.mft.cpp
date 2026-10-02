//type:cp
//header_unit_files:test.h
//options_all:--module_import_diagnostics -d-module_report --c++20
import "test.h";

// Setup some testing infrastructure.
template<typename a_Type_A, typename a_Type_B>
struct foo {
  constexpr static int pass = false;
};

template<typename a_Type>
struct foo<a_Type, a_Type> {
  constexpr static int pass = true;
};

// Assert that the type we got is what we expected to get back.

//
// Void
//
static_assert(foo<td_void, void>::pass);

//
// nullptr_t
//
static_assert(foo<td_nullptr_t, decltype(nullptr)>::pass);

//
// Integral Types
//

// Plain
static_assert(foo<td_bool,     bool>::pass);
static_assert(foo<td_char,     char>::pass);
static_assert(foo<td_wchar_t,  wchar_t>::pass);
static_assert(foo<td_char8_t,  char8_t>::pass);
static_assert(foo<td_char16_t, char16_t>::pass);
static_assert(foo<td_char32_t, char32_t>::pass);
// Signed
static_assert(foo<td_signed_char, signed char>::pass);
static_assert(foo<td_short,       short>::pass);
static_assert(foo<td_int,         int>::pass);
static_assert(foo<td_long,        long>::pass);
static_assert(foo<td_long_long,   long long>::pass);
// Unsigned
static_assert(foo<td_unsigned_char,      unsigned char>::pass);
static_assert(foo<td_unsigned_short,     unsigned short>::pass);
static_assert(foo<td_unsigned_int,       unsigned int>::pass);
static_assert(foo<td_unsigned_long,      unsigned long>::pass);
static_assert(foo<td_unsigned_long_long, unsigned long long>::pass);

//
// Float Types
//
static_assert(foo<td_float,       float>::pass);
static_assert(foo<td_double,      double>::pass);
static_assert(foo<td_long_double, long double>::pass);
