//type:fp
//options_all:--c++14 -A
//remark:[5.0] Default-const-constructible types
// 11/30/17 [EDGcpfe/18953]
//
// Default-const-constructible types
//
// The front end now implements the resolution of Core issue 253 (via paper
// P0490R0), which introduces the notion of default-const-constructible types:
// Objects of such types can be default-initialized even when const-qualified.
struct B {};
struct D: B {};  // D and B are default-const-constructible.
D const cd;      // Previously an error in strict mode because of a missing
                 // initializer.  Now okay (there aren't any members to
                 // initialize).
