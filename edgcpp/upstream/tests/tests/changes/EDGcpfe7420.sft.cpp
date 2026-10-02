//type:fp
//remark:[4.2] C++-generating back end: deprecated string literal conversion
// 12/23/09 [EDGcpfe/7420]
//
// C++-generating back end: deprecated string literal conversion
//
// C++ permits use of a string literal (which has type "array of const char"
// or "array of const wchar_t") in a context requiring a pointer to non-const
// char or wchar_t, respectively.  This (deprecated) implicit conversion
// appears in the IL as a compiler-generated cast, and the C++-generating back
// end makes the cast explicit in the generated code to allow for cases in
// which the target compiler may not allow the implicit conversion.  This cast
// was omitted, however, for wide string literals and in cases where the
// target type was a pointer to a typedef for char instead of a pointer
// directly to char.  This is now fixed so that the casts appear in these
// cases.
typedef char T;
void f() {
  T* x = "";         // now generated as ((T *)("")),
  wchar_t* y = L"";  // now generated as ((wchar_t *)(L""))
}
