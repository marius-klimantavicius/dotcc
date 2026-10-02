//type:fp
//options_all:--g++
//remark:[4.5] GNU compatibility: attribute "packed" on functions, parameters, and typedefs
// 5/31/12  [EDGcpfe/12952,EDGcpfe/12953]
//
// GNU compatibility: attribute "packed" on functions, parameters, and typedefs
//
// In GNU modes, the front end now ignores the "packed" attribute on the
// declarations of functions, parameters, and typedefs.  (Previously, such usage
// resulted in errors.)
__attribute((packed)) void f();  // Now accepted with a warning.
                                 // Previously an error.
