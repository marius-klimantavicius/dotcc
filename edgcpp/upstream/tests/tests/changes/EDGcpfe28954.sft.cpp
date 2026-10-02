//type:fp
//options_all:--c++26
//remark:C++26: value of __cplusplus
// 7/14/26  [EDGcpfe/28954]
//
// C++26: value of __cplusplus
//
// The value of the predefined __cplusplus macro in non-emulation C++26 modes
// has been changed from its placeholder value of 202600L to the value given
// in the C++26 Draft International Standard, 202603L.  As of this writing,
// g++ (16.1.0) and clang (22.1.x) use the placeholder value of 202400L, and
// MSVC (19.52) uses 199711L; the front end follows suit in its respective
// emulation modes.
static_assert(__cplusplus == 202603L, "");
