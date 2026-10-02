//type:fp
//options_all:--microsoft
//remark:[4.5] Microsoft compatibility: Calling conventions and lambda expressions
// 4/5/12   [EDGcpfe/12821]
//
// Microsoft compatibility: Calling conventions and lambda expressions
//
// In Microsoft mode, the closure type for a no-capture lambda now includes
// multiple conversion functions to function pointers: One each for the __cdecl,
// __stdcall, and __fastcall conventions.  If C++/CLI mode is enabled, the
// __clrcall convention is also added to that list.
//
// (See also the Changes entry for [EDGcpfe/10612,EDGcpfe/11770,EDGcpfe/12243] of
typedef int (__stdcall *FP1)();
typedef int (__cdecl *FP2)();
auto x = []{ return 42; };
FP1 fp1 = x;
FP2 fp2 = x;  // Now both initializations are accepted.
