//type:fp
//options_all:--ms_extensions --ms_extensions --ms_extensions
//remark:Out-of-range enumerator values with --ms_extensions
// 3/11/26  [EDGcpfe/25488,EDGcpfe/27895]
//
// Out-of-range enumerator values with --ms_extensions
//
// In Microsoft mode, enumerator constants have type "int", and out-of-range
// enumerator constant values are truncated if needed.  Previously, the second
// part was not done in non-Microsoft modes with --ms_extensions, but the first
// part still applied.  This resulted in error with an example like
//
// (assuming a 32-bit int type) because 4'000'000'000 is outside the range of
// type int.  That is now fixed.  Furthermore, this nonstandard behavior is now
// entirely avoided in non-permissive Microsoft modes.
enum E { e = 4'000'000'000 };
