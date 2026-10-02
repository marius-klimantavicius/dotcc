//type:fp
//options_all:--clang --ms_extensions
//remark:[6.0] Clang compatibility: Ignoring ill-formed attributes before extern "C"
// 9/23/19  [EDGcpfe/20875]
//
// Clang compatibility: Ignoring ill-formed attributes before extern "C"
//
// Attributes may not appear before a C++ linkage specification, but clang
// appears to accept and ignore such ill-formed attributes.  The front end now
// issues a warning in such cases in clang emulation mode (as it also currently
// does in Microsoft emulation mode).
__declspec() extern "C" void f();   // now a warning in some modes
