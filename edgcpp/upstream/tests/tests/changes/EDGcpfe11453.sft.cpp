//type:fp
//options_all:--g++
//remark:[4.3] Invalid IL when promoting a static variable that refers to a GNU address label
// 3/16/11  [EDGcpfe/11453, EDGcpfe/11492]
//
// Invalid IL when promoting a static variable that refers to a GNU address label
//
// In cases where a local static variable is initialized to a constant that
// contains a GNU address label, and lowering determines that variables in the
// scope are to be promoted to the file scope, invalid IL (a file scope constant
// that pointed into the function scope) had been generated, resulting in various
// errors (e.g., "remap_ptr_to_entry_number: pointer in secondary trans unit")
// depending on the configuration.  The initialization of promoted static
// variables that refer to GNU address labels is now re-written as executable
// code, avoiding the memory region issue.
void f() {
  struct A { A() {} };
  static void *x = &&label;
label: ;
}
