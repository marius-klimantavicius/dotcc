//type:fp
//options_all:--c++11
//remark:[4.10] Spurious diagnostics on qualifiers in alias declarations
// 11/4/14  [EDGcpfe/14286,EDGcpfe/14868,EDGcpfe/15621]
//
// Spurious diagnostics on qualifiers in alias declarations
//
// The front end had issued spurious errors on trailing cv- and ref-qualifiers
// when used in an alias declaration of a function.  Now fixed.
// (with --c++11):
using f1 = void() const;
using f2 = void() &&;
using f3 = void() const &;
