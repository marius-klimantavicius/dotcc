//type:fn
//options:--c++20
//options_all:--clang_version 220100 -w

typedef int __v16si __attribute__((__vector_size__(64)));
typedef float __v16f __attribute__((__vector_size__(64)));

void f(__v16si v16si, __v16f v16f)
{
  __builtin_elementwise_fshl(v16si, v16si);
  __builtin_elementwise_fshl(v16f, v16f, v16f);
  __builtin_elementwise_fshl(v16si, v16si, v16si, v16si);

  __builtin_elementwise_fshr(v16si, v16si);
  __builtin_elementwise_fshr(v16f, v16f, v16f);
  __builtin_elementwise_fshr(v16si, v16si, v16si, v16si);

  __builtin_elementwise_clzg();
  __builtin_elementwise_clzg(v16f);
  __builtin_elementwise_clzg(v16f, v16f);
  __builtin_elementwise_clzg(v16si, v16si, v16si);
  __builtin_elementwise_ctzg();
  __builtin_elementwise_ctzg(v16f);
  __builtin_elementwise_ctzg(v16f, v16f);
  __builtin_elementwise_ctzg(v16si, v16si, v16si);
}
