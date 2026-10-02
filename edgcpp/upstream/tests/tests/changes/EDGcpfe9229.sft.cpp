//type:fp
//options_all:--g++
//remark:[4.0] GNU compatibility: Attributes in casts
// 9/26/08  [EDGcpfe/9229]
//
// GNU compatibility: Attributes in casts
//
// In GNU mode, the front end previously issued an error when encountering an
// attribute on the type name in a cast.  Now, such attributes are accepted:
// If applicable, they modify the type; otherwise, they are ignored with a
// warning.
//
// This change also applies to other type names -- like template type arguments
// -- that aren't directly specifying the type of a declaration.
long f(float x) {
  return (__attribute((mode(DI), deprecated)) int)x;
  // The "mode" attribute is applied to "int"; the "deprecated" attribute
} // is ignored with a warning.
