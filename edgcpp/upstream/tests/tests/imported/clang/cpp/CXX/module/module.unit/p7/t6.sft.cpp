//type: fp
//options:  --c++20: --c++20
# 1 "CXX/module/module.unit/p7/t6.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 493 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "CXX/module/module.unit/p7/t6.cpp" 2





module;
# 1 "CXX/module/module.unit/p7/Inputs/h2.h" 1
extern "C++" class CPP;
# 8 "CXX/module/module.unit/p7/t6.cpp" 2
export module use;
import X;
void printX(CPP *cpp) {
  cpp->print();
}
