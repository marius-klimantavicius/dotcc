//type:fp
//options_all:--c++17 -tused -A
//
  int a;
  int h(int a, int b = sizeof(a)); //OK, unevaluated operand
