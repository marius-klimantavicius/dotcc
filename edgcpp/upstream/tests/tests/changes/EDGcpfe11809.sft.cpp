//type:fp
//options_all:--g++
//remark:[4.4] GNU compatibility: Attribute "format" on function parameters
// 6/17/11 [EDGcpfe/11809]
//
// C++-generating back end: Postfix attributes on parameters
//
// The C++-generating back end previously failed to render postfix attributes
// on parameter declarations.  This is now fixed.
//
// 6/17/11 [EDGcpfe/11809]
//
// GNU compatibility: Attribute "format" on function parameters
//
// In GNU modes, the front end now accepts the attribute "format" on function
// parameters.
void g(void (*pf) (void *, const char *, ...)
                 __attribute((format(printf, 2, 3)))) {
  /* Now accepted in GNU modes. */
}
