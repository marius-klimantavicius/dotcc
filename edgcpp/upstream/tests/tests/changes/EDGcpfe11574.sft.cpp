//type:fp
//options_all:--g++
//remark:[4.5] GNU compatibility: Attribute "nonnull" on function parameters
// 8/16/12  [EDGcpfe/11574]
//
// GNU compatibility: Attribute "nonnull" on function parameters
//
// The front end previously failed to accept the "nonnull" attribute on function
// parameter declarations.  This is now fixed.
//
// (See also the Changes entry for EDGcpfe/11809 on 6/17/11 for a similar change
// applied to the "format" attribute.)
void g(void (*pf)(char const*) __attribute((nonnull(1)))) {
          // The declaration of pf previously elicited an error.
  pf(0);  // Now a warning, because of the effect of the attribute.
}
