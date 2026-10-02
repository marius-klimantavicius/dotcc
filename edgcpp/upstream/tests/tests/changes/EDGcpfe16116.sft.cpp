//type:fp
//options_all:--g++ --c++11
//remark:[4.10.1] GNU compatibility: Spurious error after unrecognized attribute
// 3/31/15  [EDGcpfe/16116]
//
// GNU compatibility: Spurious error after unrecognized attribute
//
// In configurations where RECORD_UNRECOGNIZED_ATTRIBUTES is TRUE, an
// unrecognized attribute before an "auto" type specifier with a trailing
// return had resulted in a spurious error.  Now fixed.
// --g++ --c++11:
inline __attribute__((X)) auto f() -> int;  // previously a spurious error
