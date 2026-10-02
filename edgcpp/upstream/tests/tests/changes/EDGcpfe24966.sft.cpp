//type:fp
//options_all:--g++ --c++14
//remark:[6.4] Incorrect value for __builtin_memcmp during interpretation in GNU mode
// 1/5/22   [EDGcpfe/24966]
//
// Incorrect value for __builtin_memcmp during interpretation in GNU mode
//
// As a result of the changes for EDGcpfe/24283 (in version 6.3), the interpreter
// had returned 0 when __builtin_memcmp was invoked with two arguments that
// pointed to two different locations within the same object (only in GNU
// emulation mode).  Now fixed.
constexpr const char* a = "abcdef";
static_assert(__builtin_memcmp(a, a+1, 2) < 0);
