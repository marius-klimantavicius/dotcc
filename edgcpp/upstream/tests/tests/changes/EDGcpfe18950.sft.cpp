//type:fp
//options_all:--clang
//remark:[5.0] clang compatibility: __is_identifier
// 11/21/17 [EDGcpfe/18950]
//
// clang compatibility: __is_identifier
//
// The front end now supports the clang __is_identifier feature-test macro.
// It takes a single token as an argument and has the value 1 if the token is
// an identifier and 0 if it is anything else (e.g., a keyword).
// with --clang:
int i = __is_identifier(int);  // Expands to 0
int j = __is_identifier(abc);  // Expands to 1
