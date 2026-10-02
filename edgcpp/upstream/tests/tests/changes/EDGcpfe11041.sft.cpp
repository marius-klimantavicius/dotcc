//type:fp
//options_all:--g++
//remark:[4.3] GNU compatibility: Behavior of gnu_inline
// 10/18/10 [EDGcpfe/11041,EDGcpfe/11074,EDGcpfe/11083]
//
// GNU compatibility: Behavior of gnu_inline
//
// The primary purpose of the GNU gnu_inline attribute is to indicate that
// GNU C89 semantics should be associated with the GNU __inline keyword (see
// Changes entry of 9/29/09).  Now it also allows an inline function definition
// to be replaced by a later non-inline definition in GNU C++ mode (a warning
// is still issued).  E.g.:
//
// Also, previously a function first declared without the gnu_inline attribute
// and later redeclared with that attribute always resulted in an error.  Now,
// an error is issued only if the original declaration made the function inline.
void g();
__attribute((gnu_inline)) __inline void g() {}
  // Previously an error because the original declaration of g does not
  // specify the attribute; now accepted because the original declaration
  // was not inline.
