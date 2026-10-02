//type:fp
//options_all:--c++11
//remark:[4.11] Attributes on a using-directive
// 10/1/15  [EDGcpfe/16531]
//
// Attributes on a using-directive
//
// A spurious error had been given for attributes that preceded a using-directive.
// That restriction has been lifted except in GNU emulation modes.  Attributes are
// still disallowed for using-declarations (in all modes).
// --c++11):
namespace A {}
[[ ]] using namespace A;  // Now allowed (except in GNU emulation mode).
