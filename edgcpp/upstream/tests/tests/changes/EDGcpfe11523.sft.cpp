//type:fp
//remark:[4.4] Abort on attribute with string literal argument
// 5/23/11  [EDGcpfe/11523]
//
// Abort on attribute with string literal argument
//
// The front end previously sometimes recorded a string literal argument to an
// attribute in a function-scope memory region, resulting in invalid IL since an
// attribute (always stored in file-scope memory region) would end up pointing
// into function-scope memory.  This could lead to aborts, in particular when
// writing the IL to file.
//
// This bug, introduced in version 4.2, is now fixed.
void f() {
  struct __declspec(uuid("00000000-0000-0000-0000-000000000000")) S;
    // The attribute (stored in file-scope memory) ended up pointing to
    // a string constant stored in function-scope memory.
}
