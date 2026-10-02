//type:fp
//options_all:--c++11
//remark:[4.7] C++11 raw string literals
// 4/9/13   [EDGcpfe/8626]
//
// C++11 raw string literals
//
// We have now implemented the "raw string literal" feature of C++11 (see
// papers N2442 and N3077).  These literals can span multiple lines and
// escape sequences and trigraphs are not translated.
const char *p = R"+++(a\
??=\0xa
z)+++";  // Equivalent to "a\\\n\?\?=\\0xa\nz"
