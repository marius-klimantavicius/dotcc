//type:fp
//options_all:--gcc
//remark:[6.6] GNU and Clang C compatibility: Overlong string initializers
// 8/31/23  [EDGcpfe/26610]
//
// GNU and Clang C compatibility: Overlong string initializers
//
// The diagnostic severity of character arrays initialized with overlong strings
// is now reduced to a warning in Clang and GNU C modes.
//
// In such cases, the string initializer is truncated to the length of the
// destination array (the terminating null character is lost).
char const str[1] = "xyz";  // Now accepted with a warning in some modes.
