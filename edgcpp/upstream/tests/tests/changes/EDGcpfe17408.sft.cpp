//type:fp
//options_all:--c++17
//remark:[4.12] C++17 compatibility: terse static_assert
// 7/20/16  [EDGcpfe/17408]
//
// C++17 compatibility: terse static_assert
//
// Paper N3928 introduced the "terse" static_assert, i.e., a static_assert with
// only a single argument.  That is now implemented in C++17 mode, as well as
// when microsoft_version >= 1903 and --ms_c++latest is specified, or when
// gnu_version >= 60000 and C++11 mode (or later) is specified.
static_assert(true);    // Now accepted in various modes.
