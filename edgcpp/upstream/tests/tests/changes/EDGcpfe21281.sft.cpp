//type:fp
//options_all:--c++11
//remark:[5.1] Auto type specifier flag on function pointer with trailing return type
// 5/23/19  [EDGcpfe/21281]
//
// Auto type specifier flag on function pointer with trailing return type
//
// Function pointer variables declared with trailing return types were incorrectly
// having their declared_with_auto_type_specifier flag set.  This caused problems
// when using that variable.
//
// This is now fixed.
int f(void);
auto (*g)(void) -> int;
int h() {
  g = f;      // Spurious "auto variable in its own initializer" error
  return g(); // Spurious "auto variable in its own initializer" error
}
