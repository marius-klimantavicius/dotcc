//type:fp
//options_all:--c++20 --no_strict --c++20
//remark:Abbreviated function template syntax for deduction guides
// 11/27/25 [EDGcpfe/24985,EDGcpfe/25973,EDGcpfe/26112,EDGcpfe/28367]
//
// Abbreviated function template syntax for deduction guides
//
// The resolution of Core issue 2697 clarified that a deduction guide cannot be
// declared using abbreviated function template syntax.  However, as this is
// accepted by GCC, Clang, and MSVC, the front end now also accepts it in
// non-strict modes.
//
// This change also fixes an issue parsing constrained generic parameters during
// disambiguation.
template<typename>
struct C {
  C(int);
};
C(auto i) -> C<decltype(i)>;  // Previously an error, now accepted in
                              // non-strict modes.
