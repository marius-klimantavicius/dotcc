//type:fp
//options_all:--c++14
//remark:[4.9] C++14: [[deprecated]]
// 1/6/14   [EDGcpfe/14685]
//
// C++14: [[deprecated]]
//
// In C++14 mode, the front end now recognizes the standard-notation attribute
// "deprecated", optionally followed by a parenthesized string literal.  Uses of
// an entity marked with this attribute are diagnosed with a warning by default.
//
// (Similar attributes with Microsoft- and GNU-like syntax were already supported
// by the front end.)
int x [[deprecated("Old!")]];  // This attribute is now recognized in C++14
                               // mode.
int y = x+1;                   // A warning is issued for the use of x here.
