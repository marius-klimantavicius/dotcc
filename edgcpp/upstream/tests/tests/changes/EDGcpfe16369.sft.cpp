//type:fp
//options_all:--c++11
//remark:[4.11] Member access expressions in constant expressions
// 7/12/15  [EDGcpfe/16369]
//
// Member access expressions in constant expressions
//
// In contexts requiring a constant expression, the front end in C++11 mode
// incorrectly rejected member access expressions involving member enumerators
// and static data members when the object expression is not an address
// constant.  This is now fixed.
struct X {
  enum Y { A };
};
void f(X::Y y) {
  X x;
  switch (y) {
    case x.A:   // Previously incorrectly rejected
      break;
  }
}
