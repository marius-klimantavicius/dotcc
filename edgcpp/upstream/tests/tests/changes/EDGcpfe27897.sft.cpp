//type:fp
//options_all:--ms_extensions --ms_extensions --gcc
//remark:GNU/Clang compatibility: packed attribute on enums with --ms_extensions
// 12/12/25 [EDGcpfe/27897,EDGcpfe/28594]
//
// GNU/Clang compatibility: packed attribute on enums with --ms_extensions
//
// The front end now correctly sizes enum types with the packed attribute in
// GNU/Clang modes when --ms_extensions is specified.
// --ms_extensions):
enum E { e } __attribute__((__packed__));
_Static_assert(sizeof(enum E) == 1, "");
