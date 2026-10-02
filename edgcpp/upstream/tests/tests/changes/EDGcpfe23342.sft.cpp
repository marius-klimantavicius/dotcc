//type:fp
//options_all:--c++17
//remark:[6.2] C++-generating back end: lambda that captures and uses *this
// 9/13/20  [EDGcpfe/23342]
//
// C++-generating back end: lambda that captures and uses *this
//
// The front end previously aborted with a failed assertion (in gen_expr) when
// generating code for a lambda that captures and then uses *this.  This is
// now fixed.
struct S {
  void foo(){
    [*this]() { S x = *this; };   // Previously aborted
  }
};
