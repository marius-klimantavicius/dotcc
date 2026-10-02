//type:fp
//options_all:--microsoft_v 1936 --ms_c++20 --targ win64
//remark:[6.7] Microsoft compatibility: 64-bit targets and calling conventions
// 5/30/24  [EDGcpfe/26810]
//
// Microsoft compatibility: 64-bit targets and calling conventions
//
// Previously, this elicited an error.  However, for 64-bit targets the __stdcall,
// __fastcall, and __cdecl calling convention are all equivalent to the default,
// and thus MSVC accepts such code.  The front end now does as well.
int __stdcall __fastcall g();
