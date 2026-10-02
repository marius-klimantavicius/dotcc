//type:fp
//options_all:--c++11 --g++
//remark:[4.12] Nested class defined in alias declaration not handled correctly
// 6/29/16  [EDGcpfe/17343]
//
// Nested class defined in alias declaration not handled correctly
//
// The front end previously treated nested classes defined in alias declarations
// as if they weren't nested at all if the alias appeared in a class template.
// This in turn was likely to lead to spurious errors.
//
// This is now fixed.
template<typename T> struct X {
  using A = struct N {};  // Previously N was not considered a member of
                          // X<T>.
  A m;  // A spurious error was issued here claiming m's type is incomplete.
};
X<void> x;
