//type:fp
//options_all:--g++ --c++11
//remark:[6.2] GCC compatibility: accept nullptr in call to sync/atomic builtins
// 10/26/20 [EDGcpfe/19494,EDGcpfe/20574,EDGcpfe/22687,EDGcpfe/23470]
//
// GCC compatibility: accept nullptr in call to sync/atomic builtins
//
// For some sync/atomic builtins, GCC/clang will accept an argument with pointer
// type for an integer parameter (as long as the pointer and integer are the same
// size).  That has now been extended to allow nullptr.
// (with --g++ --c++11):
#define ATOMIC_ACQUIRE 2
void f() {
  int* p = nullptr;
  __atomic_exchange_n(&p, (void*)0, ATOMIC_ACQUIRE); // Previously okay.
  __atomic_exchange_n(&p, nullptr, ATOMIC_ACQUIRE);  // Now also okay.
}
