//type:fp
//options_all:--c++11
//remark:[4.8] C++11: User-defined conversions to std::nullptr_t and comparison operators
// 6/20/13  [EDGcpfe/14168]
//
// C++11: User-defined conversions to std::nullptr_t and comparison operators
//
// In C++11 modes, the front end now considers user-defined conversions to
// std::nullptr_t when resolving relational and equality operators.
//
// Note that for relational operators (e.g., "<="; but not for "==" or "!=")
// the next standard will likely make comparisons of nullptr_t expressions
// invalid altogether.
namespace std { using nullptr_t = decltype(nullptr); }
struct X { operator std::nullptr_t() { return nullptr; } } x;
bool b = x<x;  // Previously an error; now accepted.
