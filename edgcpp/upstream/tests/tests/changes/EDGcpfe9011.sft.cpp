//type:fp
//remark:[4.1] Diagnostic output with incorrect number of arguments in a macro invocation
// 5/28/09  [EDGcpfe/9011]
//
// Diagnostic output with incorrect number of arguments in a macro invocation
//
// The diagnostic issued by the front end when too many arguments are supplied
// in a macro invocation previously omitted the name of the macro, which was
// confusing in cases when the invocation appears in expanded macro text
// rather than directly in the source.
//
// The diagnostic now includes the name of the macro, "foo".  Also, the caret
// in the output with --macro_positions_in_diagnostics formerly pointed to the
// "3" in the definition of "foo2" (the source position of the text being
// passed as the extra argument).  This has now been changed to point to "foo"
// in the definition of "foo1".  The name of the macro is now also included
// when too few arguments are supplied in a macro invocation.
#define foo(a,b) a+b
#define foo1(a,b,c) foo(a,b,c)
#define foo2 foo1(1,2,3)

int f() {
  return foo2;
}
