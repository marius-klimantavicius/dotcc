//type: fn
//options:  --c++20 -DINTERFACE: --c++20 -DIMPLEMENTATION: --c++20
# 1 "CXX/basic/basic.scope/basic.scope.namespace/p2.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 463 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "CXX/basic/basic.scope/basic.scope.namespace/p2.cpp" 2
# 31 "CXX/basic/basic.scope/basic.scope.namespace/p2.cpp"
void test_early() {
  in_header = 1;


  global_module_fragment = 1;

  exported = 1;

  not_exported = 1;


  internal = 1;

  not_exported_private = 1;

  internal_private = 1;
}




import A;


void test_late() {
  in_header = 1;


  global_module_fragment = 1;

  exported = 1;

  not_exported = 1;





  internal = 1;

  not_exported_private = 1;





  internal_private = 1;
}
