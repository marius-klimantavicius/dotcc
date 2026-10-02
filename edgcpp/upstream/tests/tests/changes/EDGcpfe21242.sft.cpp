//type:fp
//options_all:--microsoft -w --c++ --microsoft_version 1920 --ms_c++17
//remark:[5.1] Microsoft compatibility: Empty classes and constant expressions
// 6/18/19  [EDGcpfe/21242]
//
// Microsoft compatibility: Empty classes and constant expressions
//
// The changes for EDGcpfe/18739 (see entry of 2/21/18) allowed the trivial
// copying of empty class objects as part of constant expressions even when those
// empty objects are "non-constant run-time" objects.  However, that change was
// not enabled in Microsoft mode.  Now, it is enabled in Microsoft C++ modes with
// microsoft_version >= 1914.  Note that this can also apply to closure types.
auto closure = [](auto p) { return p; };
constexpr auto r = closure; // Now accepted in some Microsoft C++17 modes.
