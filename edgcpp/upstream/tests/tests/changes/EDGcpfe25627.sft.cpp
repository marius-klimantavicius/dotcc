//type:fp
//options_all:--gnu=90300
//remark:[6.5] Ignoring top-level qualifiers on calls to __builtin_shuffle
// 11/23/22 [EDGcpfe/25627]
//
// Ignoring top-level qualifiers on calls to __builtin_shuffle
//
// A change has been made to ignore top-level qualifiers for arguments of
// __builtin_shuffle.
typedef int   VI __attribute__((vector_size(4)));
typedef float VF __attribute__((vector_size(4)));
void f(const VF *a1, VF *a2, VI m) {
  (void)__builtin_shuffle (*a1, *a2, m);  // Now compiles
}
