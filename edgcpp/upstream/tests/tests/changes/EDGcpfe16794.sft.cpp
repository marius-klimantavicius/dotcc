//type:fn
//options_all:--c++11
//remark:[4.11] Address of constexpr local variable in constexpr initializer
// 1/22/16  [EDGcpfe/16794]
//
// Address of constexpr local variable in constexpr initializer
//
// The front end previously failed to diagnose an initializer for a constexpr
// variable that was the address of a local constexpr variable.
//
// This is now fixed.
int main() {
  constexpr int i = 0;
  constexpr const int *p = &i;  // Previously accepted in C++11 mode.
}                               // Now an error.
