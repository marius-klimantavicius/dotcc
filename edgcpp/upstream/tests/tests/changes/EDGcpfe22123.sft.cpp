//type:fp
//options_all:--clang
//remark:[6.1] C++-generating back end: segfault for trivial destructor call via local typedef
// 12/14/19 [EDGcpfe/22123]
//
// C++-generating back end: segfault for trivial destructor call via local typedef
//
// In C++-generating back end configurations in which
// clang_is_generated_code_target is TRUE, the changes for EDGcpfe/21335 (in
// version 5.1) resulted in dereferencing a null pointer (in
// scope_is_in_name_context_stack) when a trivial destructor is invoked using a
// local typedef name.  This is now fixed.
void f() {
  typedef struct { } S;
  S s;
  s.~S();  // Previously a segfault, now okay.
}
