//type:fp
//options_all:--c++14 --g++
//remark:[6.0] Spurious error for class defined in statement expression in function template
// 8/15/19  [EDGcpfe/21631]
//
// Spurious error for class defined in statement expression in function template
//
// The changes for EDGcpfe/20016 (in version 5.1) resulted in the front end
// issuing a spurious error when a class is defined in the body of a GNU
// statement expression that appears within the definition of a function
// template.  This is now fixed.
template<typename> void f () {
  int a = ({ struct A{} b; 42; }); // Previously a spurious error, now okay
}
