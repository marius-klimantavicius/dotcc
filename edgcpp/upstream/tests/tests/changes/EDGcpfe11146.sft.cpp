//type:fp
//options_all:--g++
//remark:[4.3] GNU C++ compatibility: Attributes in conversion function names
// 1/4/11   [EDGcpfe/11146]
//
// GNU C++ compatibility: Attributes in conversion function names
//
// In GNU C++ mode, the front end now accepts a type that starts with a GNU
// attribute in the name of a conversion function.
struct S {
  operator __attribute((vector_size(16))) float();  // Now accepted.
};
