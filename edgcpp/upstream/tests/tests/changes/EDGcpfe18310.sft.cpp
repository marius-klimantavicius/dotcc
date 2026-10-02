//type:fp
//options_all:--clang_v 30900
//remark:[4.14] Clang compatibility: __type_pack_element builtin
// 5/30/17  [EDGcpfe/18310]
//
// Clang compatibility: __type_pack_element builtin
//
// When clang_version >= 30900, the __type_pack_element builtin is now enabled.
// This builtin takes at least two template arguments.  The first template
// argument is for a non-type template parameter of type size_t that specifies
// which of the remaining template arguments to select as the returned type for
// the builtin.  The index argument is zero-based.
// --clang_version 30900):
//
// __has_builtin(__type_pack_element) is now TRUE when __type_pack_element is
// available.
__type_pack_element<0, int, double, bool> i = 0;    // declares an int
__type_pack_element<1, int, double, bool> d = 1.0;  // declares a double
__type_pack_element<2, int, double, bool> b = true; // declares a bool
