//type:fp
//options_all:--microsoft_16
//remark:[4.9] Spurious error on union member when near/far qualifiers are enabled
// 10/29/13 [EDGcpfe/14612]
//
// Spurious error on union member when near/far qualifiers are enabled
//
// Version 4.8 of the front end introduced a regression causing the front end
// to issue a spurious error about invalid union members when near/far qualifiers
// are enabled (e.g., in "--microsoft_16" mode).
//
// This is now fixed.
struct C { char c; };
union U { C x; } u;  // Triggered a spurious error in version 4.8 when
                     // near/far qualifiers were enabled.
