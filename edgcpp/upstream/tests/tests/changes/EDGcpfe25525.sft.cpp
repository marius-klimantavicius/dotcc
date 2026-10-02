//type:fp
//options_all:--ms_c++20 --no_char8_t
//remark:[6.4] Disabling char8_t support in Microsoft C++20 mode
// 9/20/22  [EDGcpfe/25525]
//
// Disabling char8_t support in Microsoft C++20 mode
//
// Previously, the --no_char8_t command-line option had no effect in Microsoft
// C++20 mode.  This is now fixed.
const char *s = u8"";  // Previously an error.  Now okay.
