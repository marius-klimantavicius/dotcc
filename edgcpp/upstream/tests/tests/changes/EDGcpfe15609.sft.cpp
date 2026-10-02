//type:fp
//options_all:--c++11
//remark:[4.10] constexpr signed/unsigned char array initialized from string literal
// 12/10/14 [EDGcpfe/15609]
//
// constexpr signed/unsigned char array initialized from string literal
//
// The front end previously failed to treat a constexpr array of signed or
// unsigned char initialized from a string literal as a constant expression,
// because the element type of the array is different from the plain char type
// of the characters in the string literal.  This is now fixed.
// with --c++11:
constexpr unsigned char r[] = "abc";
static_assert(r[0] == 'a', "");   // Previously treated as non-constant
