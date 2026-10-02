//type:fp
//options_all:--g++ --c++17
//remark:[5.1] GNU and clang compatibility: __has_unique_object_representations
// 12/11/18 [EDGcpfe/19940,EDGcpfe/20165,EDGcpfe/20317]
//
// GNU and clang compatibility: __has_unique_object_representations
//
// The front end previously gave different results in g++ and clang modes than
// those of the emulated compilers for some types involving bit-fields, tail
// padding, arrays, and vectors.  These are now fixed.
// --g++ --c++17:
union U { int i : sizeof(int) * __CHAR_BIT__ - 1; };
static_assert(!__has_unique_object_representations(U), ""); // Now okay
