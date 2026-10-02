//type:fp
//options_all:--clang --c++11
//remark:[4.12] GNU/clang C++ compatibility: designated initializers and constexpr
// 5/19/16  [EDGcpfe/16807]
//
// GNU/clang C++ compatibility: designated initializers and constexpr
//
// The front end previously did not fully support the g++ and clang extension
// of designated initializers in constexpr initializers: the initialization
// was permitted, but the resulting object could not be used in a constant
// expression.  This capability has now been added.
// --c++11:
constexpr struct {
  int x;
} foo[] = {
  [0] = { 0 }
};
constexpr int bar = foo[0].x;  // Previously rejected as not constant
