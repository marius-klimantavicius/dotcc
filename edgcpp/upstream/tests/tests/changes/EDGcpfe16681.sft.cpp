//type:fp
//options_all:--c++11
//remark:[4.11] Alignment of a variable or field
// 12/11/15 [EDGcpfe/16681]
//
// Alignment of a variable or field
//
// In nonstrict C++11 modes, the front end accepts (with a warning) the alignof
// operator applied to an expression (as opposed to a type).  However, if the
// expression designates a variable or field with an explicit alignas specifier,
// it ignored that specifier, unless GNU or (for variables only) Microsoft modes
// were in effect.  Now, such a specifier affects the result of the alignof
// operator in all nonstrict C++11 modes.
alignas(16) int x[4];
static_assert(alignof(x) == 16, "Unexpected");
  // Previously failed in default C++11 mode (assuming int has an
  // alignment other than 16).  Now okay.
