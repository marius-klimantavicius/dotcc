//type:fp
//options_all:--c++11 --g++
//remark:[4.9] GNU C++11-mode IL corruption with array rvalue in constexpr context
// 2/27/14  [EDGcpfe/14890]
//
// GNU C++11-mode IL corruption with array rvalue in constexpr context
//
// In GNU C++11 mode, the front end could end up creating corrupted IL entries
// when performing an array-to-pointer conversion on an array rvalue in a
// constexpr context.  This corruption usually manifested itself as an abort
// somewhere downstream from the conversion process (e.g., when writing the IL
// to a file).
//
// In this example, the sub-expression "S{{&i}}.ap" is an array lvalue, and
// applying the subscripting operator to it implies an array-to-pointer
// conversion, which triggered an abort in GNU C++11 mode.
//
// This is now fixed.
struct S {
  int const *ap[2];
};
void g() {
  constexpr int i = 42;
  constexpr int k = *S{{&i}}.ap[0];  // Previously triggered an abort in
}                                    // some configurations.
