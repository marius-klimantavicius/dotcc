//type:fp
//options_all:--gnu=120100
//remark:[6.8] GNU/Clang compatibility: __FILE_NAME__ predefined macro
// 5/20/25  [EDGcpfe/27316,EDGcpfe/28183]
//
// GNU/Clang compatibility: __FILE_NAME__ predefined macro
//
// As of gcc version 12.1.0 and clang version 9.0.0, these compilers support
// the __FILE_NAME__ predefined macro.  This macro is like the standard
// __FILE__ macro except that the value is only the file name portion,
// excluding any directory components of the file path.  The front end now
// emulates this behavior in the relevant modes.
// file specified as /x/y/z.c with --gnu_version=120100:
const char *p = __FILE_NAME__;   // Equivalent to "z.c"
