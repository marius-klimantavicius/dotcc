//type:fn
//options_all:--microsoft_v 1921 --ms_c++17
//remark:[6.1] Microsoft C++ compatibility: __is_constructible (etc.) and destructors
// 12/6/19  [EDGcpfe/22087]
//
// Microsoft C++ compatibility: __is_constructible (etc.) and destructors
//
// The changes for EDGcpfe/17852 made __is_constructible produce a false value if
// __is_destructible produced a false value for the given type.  However, an
// exception was made in Microsoft mode, because Microsoft compilers at the time
// did not behave that way.  Now, the Microsoft-mode exception only exists when
// microsoft_version <= 1910.  This also affects __is_nothrow_constructible and
// __is_trivially_constructible.
struct N { ~N(); };
static_assert(__is_trivially_constructible(N), "");
  // Now an error in Microsoft modes with microsoft_version > 1910.
