//type:fp
//options_all:--microsoft
//remark:[4.0] Microsoft compatibility: Inherited const member functions named as friends
// 11/18/08 [EDGcpfe/9355]
//
// Microsoft compatibility: Inherited const member functions named as friends
//
// In Microsoft mode, the front end allows friend declarations to refer to
// inherited member functions of a class (see Changes entry of 8/4/00).  However,
// a spurious error was issued if the friend declaration was for a const (or
// volatile) member function.
//
// This is now fixed.
struct B { void f() const; };
struct D: B {};
struct X {
  friend void D::f() const;  // Triggered a spurious type compatibility
};                           // error in Microsoft C++ mode.
