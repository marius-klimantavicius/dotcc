//type:fp
//options_all:--g++
//remark:[6.8] GNU compatibility: Pointer arguments to __atomic_... builtins
// 8/8/25   [EDGcpfe/20813,EDGcpfe/27780,EDGcpfe/28376]
//
// GNU compatibility: Pointer arguments to __atomic_... builtins
//
// GCC allows the "value" parameter of __atomic_... builtins to also have pointer
// type.
void f(char *p, char *v) {
  __atomic_store_n(p, v, 0);  // Now accepted in GNU mode.
}
