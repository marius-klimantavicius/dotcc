//type:fp
//options_all:--g++
//remark:[4.1] GNU compatibility: V16 vector mode name prefix
// 2/27/09  [EDGcpfe/9313, EDGcpfe/9575]
//
// GNU compatibility: V16 vector mode name prefix
//
// In GNU modes, the front end now accepts "V16" as a vector mode name prefix.
typedef int v16qi __attribute((mode(V16QI)));  // Now accepted in GNU modes.
