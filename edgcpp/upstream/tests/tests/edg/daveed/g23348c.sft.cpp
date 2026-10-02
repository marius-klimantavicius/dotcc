//remark:ext_vector_type conversion
//options:--clang;fp

typedef char vec_t __attribute__((__ext_vector_type__(8)));
void foo() {
  vec_t v(0);
  v = 42;
}

