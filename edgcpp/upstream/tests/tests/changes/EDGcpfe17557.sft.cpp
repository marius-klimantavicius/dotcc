//type:fp
//options_all:--gn 60100
//remark:[6.5] Vector operations with cv-qualifier differences
// 2/27/23  [EDGcpfe/17557,EDGcpfe/26089]
//
// Vector operations with cv-qualifier differences
//
// A change has been made to ignore cv-qualifier differences in the types of
// operands of a vector operation.
void f(      int __attribute__((vector_size(16))) a,
       const int __attribute__((vector_size(16))) b) {
  (void)(a + b);
}
