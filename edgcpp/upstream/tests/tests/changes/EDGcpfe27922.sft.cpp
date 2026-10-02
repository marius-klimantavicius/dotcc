//type:fp
//options_all:--g++
//remark:[6.8] GNU compatibility: vector_size attribute
// 2/6/25   [EDGcpfe/27922]
//
// GNU compatibility: vector_size attribute
//
// The vector_size attribute is typically only applicable to integral and floating
// scalars, but GNU allows pointer types (though Clang does not).
// (with --g++):
int *x [[gnu::vector_size(16)]];
