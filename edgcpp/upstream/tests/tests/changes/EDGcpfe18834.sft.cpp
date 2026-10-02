//type:fp
//remark:[5.0] Failure to fold initializer for static-lifetime variable
// 10/3/17  [EDGcpfe/18834]
//
// Failure to fold initializer for static-lifetime variable
//
// In C++ modes, the front end sometimes failed to fold an initializer expression
// for a static-lifetime variable, thereby causing it to use dynamic
// initialization when static initialization is required.
//
// This regression, introduced in version 4.13, is now fixed.
extern char const C = "x"[0];
int z[C];  // An error in versions 4.13 and 4.14.  Now okay.
