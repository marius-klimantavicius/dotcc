//type:fp
//options_all:--microsoft_version 1900 --c
//remark:[4.11] Microsoft compatibility: Accept "inline" specifier in C mode
// 7/22/15  [EDGcpfe/16379]
//
// Microsoft compatibility: Accept "inline" specifier in C mode
//
// When using Microsoft emulation and microsoft_version >= 1900, the "inline"
// specifier is now allowed.
inline void x() {}
