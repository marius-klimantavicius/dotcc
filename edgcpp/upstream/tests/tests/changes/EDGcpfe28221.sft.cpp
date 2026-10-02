//type:fp
//options_all:--no_const_string_literals --ms_c++20 --microsoft
//remark:[6.8] Microsoft compatibility: user-defined string literals and
// 6/10/25  [EDGcpfe/28221]
//
// Microsoft compatibility: user-defined string literals and
// --no_const_string_literals
//
// For compatibility with very old dialects of C++, the
// --no_const_string_literals command-line option (similar to MSVC's
// "/Zc:strictStrings-") causes string literals to have the type "array of X"
// instead of the standard "array of const X".  However, the type of the first
// parameter of a user-defined string literal operator must be "pointer to
// const X", so the front end issued spurious "not found" errors with that
// command-line option when user-defined string literals appear in the source.
// This is now fixed.
// --ms_c++20:
int operator"" _X(const char *, size_t);
int i = "abc"_X;   // Previously a spurious "not found" error, now okay
