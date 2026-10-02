//type:fp
//options_all:--gn 70300
//remark:[6.2] Abort with template parameters defaulted to functions
// 7/24/20  [EDGcpfe/22672]
//
// Abort with template parameters defaulted to functions
//
// In C++11 modes and beyond, the front end would abort in
// conv_function_designator_to_ptr_to_function when a template parameter is
// defaulted to a function.
//
// This is now fixed.
void foo();
template <typename Compare, Compare = foo> // Would previously abort here
class A;
