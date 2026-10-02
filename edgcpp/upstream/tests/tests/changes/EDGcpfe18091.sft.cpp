//type:fp
//options_all:--microsoft_version 1910
//remark:[4.14] Comparing string literal address in Microsoft mode
// 3/17/17  [EDGcpfe/18091]
//
// Comparing string literal address in Microsoft mode
//
// The front end usually represents identical string literals within a
// translation unit as sharing storage.  One exception is Microsoft mode.
// Now, Microsoft C++ mode (but not C mode) with microsoft_version >= 1910
// also represents string literals as sharing storage.
static_assert("a" == "a", "");  // Now accepted in some
                                // Microsoft C++ modes.
