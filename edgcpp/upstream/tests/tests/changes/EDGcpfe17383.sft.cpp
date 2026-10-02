//type:fp
//options_all:--c11 --gcc
//remark:[4.12] GCC compatibility: Allow _Noreturn on "main"
// 7/11/16  [EDGcpfe/17383]
//
// GCC compatibility: Allow _Noreturn on "main"
//
// Although the C11 standard is explicit that the _Noreturn function specifier
// should not be allowed on the "main" function, GCC (and clang) give only a
// warning, and now so does the front end.
_Noreturn int main() {}
