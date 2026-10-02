//type:fp
//options_all:--c11
//remark:[4.9] C11: _Static_assert
// 1/3/14   [EDGcpfe/12649,EDGcpfe/12864]
//
// C11: _Static_assert
//
// In C11 mode and in GNU C modes with gnu_version >= 40600, the front end now
// supports the _Static_assert feature (equivalent to the C++11 static_assert
// feature).
_Static_assert(sizeof(char) == 1, "Nonstandard char size");
  // Now accepted in C11 mode.
