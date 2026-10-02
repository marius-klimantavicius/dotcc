//type:fp
//remark:[4.3] Dropping cv-qualifiers on reinterpret_cast to pointer-to-function type
// 10/28/10 [EDGcpfe/11100]
//
// Dropping cv-qualifiers on reinterpret_cast to pointer-to-function type
//
// In GNU C++ and Microsoft C++ modes, reinterpret_cast now allows dropping
// cv-qualifiers on a cast to a pointer-to-function type.  This is nonstandard.
void foo(const void* handler) {
  reinterpret_cast<void (*)(int)>(handler);
}
