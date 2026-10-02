//type:fp
//options_all:--c++20
//remark:[6.1] C++20: Concepts
// 7/16/20  [EDGcpfe/20005,EDGcpfe/20118]
//
// C++20: Concepts
//
// The front end now supports constrained templates, concept templates, requires-
// expressions, and abbreviated function template declarations, features that are
// collectively thought of as the C++20 "concepts" feature.  The principal
// committee papers describing the changes for the standard are P0734R0 and
// P1142R2 (the latter for abbreviated function template declarations), but a
// number of additional papers added refinements to the original specification
// and most of those have been implemented as well (including P0857R0, P1084R2,
// P1452R2, P1616R1, P1972, P1980, and P2092).
//
// The IL data structures that mirror some of the front end data structures
// (a_template_parameter and a_template_decl) are now created unconditionally.
// Formerly, the were only created when all_template_info_in_il was TRUE.  This
// is a small IL CHANGE.
template<typename T> concept Sizeable = requires { sizeof(T); };
template<typename T> concept Small = Sizeable<T> && sizeof(T) < 100;
int f(Small auto p) { return 1; }   // (1)
void f(auto) {}                     // (2)
int r = f(42);  // Okay: Prefers (1).
