//type:fp
//options_all:--clang_version=130100
//remark:[6.5] Clang attribute using_if_exists
// 4/13/23  [EDGcpfe/25472,EDGcpfe/26223]
//
// Clang attribute using_if_exists
//
// The changes for EDGcpfe/25243,EDGcpfe/25501 (see entry of 8/3/22) allowed the
// front end to recognize the Clang "using_if_exists" attribute, but they did not
// cause the attribute to have any effect.  Now, the attribute inhibits an error
// that otherwise would be issued for a non-existing identifier.
//
// In such cases, no IL is recorded for the using-declaration (e.g., they are not
// rendered by the C++-generating back end).
namespace N {};
using N::X __attribute((using_if_exists));  // Previously an error because X
                                            // is not declared.  Now okay.
