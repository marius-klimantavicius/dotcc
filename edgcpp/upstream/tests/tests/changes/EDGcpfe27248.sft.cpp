//type:fp
//options_all:--c++11 --clang_v 180100
//remark:[6.7] Clang compatibility: spurious instantiation for __builtin_operator_delete
// 5/16/24  [EDGcpfe/27248]
//
// Clang compatibility: spurious instantiation for __builtin_operator_delete
//
// Previously, if __builtin_operator_delete was named using an unparenthesized,
// unqualified id, the front end would perform argument dependent lookup to find
// the corresponding deallocation function.  This could trigger template
// instantiation of any types used in function arguments.
// --c++11 --clang_version 180100:
struct A;
template<typename T>
struct C {
  T t;  // Previously a spurious error as A is still incomplete when
        // instantiated from below.
};
void f(C<A> *c) {
  __builtin_operator_delete(c);
}
