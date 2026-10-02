//type:fp
//options_all:--c++20
//remark:[5.1] C++20: Constructing closures without capture
// 8/23/18  [EDGcpfe/20013]
//
// C++20: Constructing closures without capture
//
// In C++20 mode, a closure resulting from a lambda that is introduced with "[]"
// (i.e., doesn't capture anything) now has a defaulted default constructor and
// defaulted copy/move-assignment operators (in C++17 mode these special members
// are deleted).
//
// This feature was added to the working paper for the next standard through
// paper number P0624R2.
auto cmp = [](auto x, auto y) { return x > y; };
decltype(cmp) c;  // Valid in C++20 mode (error in C++17 mode).
void g() {
  c = c;  // Valid in C++20 mode (error in C++17 mode).
}
