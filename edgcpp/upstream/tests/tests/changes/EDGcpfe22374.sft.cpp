//type:fn
//options_all:--c++11
//remark:[6.1] Failure to diagnose extra braced tokens following an in-class definition
// 3/13/20  [EDGcpfe/22374]
//
// Failure to diagnose extra braced tokens following an in-class definition
//
// In C++11 mode, the front end previously failed to diagnose brace-enclosed
// tokens following an in-class constructor definition.
//
// This is now fixed.
struct X {
  X(int i): i(i) {}
  { tokens here }  // Previously accepted.  Now an error.
  int i;
};
