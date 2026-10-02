//type:fp
//options_all:--c++17
//remark:Attributes on asm declarations
// 12/17/25 [EDGcpfe/28584]
//
// Attributes on asm declarations
//
// The front end now potentially accepts attributes on asm declarations.  This
// implements the resolution of Core issue 2262.
//
// Core issue 2262 was resolved as a feature addition for C++17, but common
// practice is to accept these attributes in C++11 mode and later: The front end
// follows suit in nonstrict modes.
[[]] asm("nop");  // Now accepted by the front end.
