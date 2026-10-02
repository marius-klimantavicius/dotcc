//type:fp
//options_all:--g++
//remark:[4.5] GNU compatibility: Severity of malformed #pragma pack directive
// 2/13/12  [EDGcpfe/12669]
//
// GNU compatibility: Severity of malformed #pragma pack directive
//
// In GNU mode, when a #pragma pack directive starts with "#pragma pack (n" not
// followed by a right parenthesis, the front end now only issues a warning
// (previously it was an error), and the pack alignment is ignored (previously
// it took effect during error recovery).
#pragma pack(2, 2)  // Previously an error; now a warning.  No packing
                    // takes effect here.
