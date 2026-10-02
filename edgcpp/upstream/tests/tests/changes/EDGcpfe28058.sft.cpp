//type:fp
//options_all:--c17 --clang_v 170999
//remark:[6.8] Additional __builtin_... functions are constant-evaluated in some C modes
// 4/4/25   [EDGcpfe/28058]
//
// Additional __builtin_... functions are constant-evaluated in some C modes
//
// The front end now constant-evaluates calls to a few more GNU-style built-in
// functions in C modes, including __builtin_COLUMN, __builtin_LINE,
// __builtin_FILE, __builtin_FILE_NAME, __builtin_FUNCTION, and __builtin_FUNCSIG.
char const *str = __builtin_FILE_NAME(); // Previously an error in GNU and
                                         // Clang C modes.  Now okay.
