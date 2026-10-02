//type: fp
//options:  --c++20
# 1 "CXX/module/module.unit/p7/t1.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 493 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "CXX/module/module.unit/p7/t1.cpp" 2


module;

# 1 "CXX/module/module.unit/p7/Inputs/h1.h" 1
extern "C" void foo();
extern "C" {
void bar();
int baz();
double double_func();
}

extern "C++" {
void bar_cpp();
int baz_cpp();
double double_func_cpp();
}
# 6 "CXX/module/module.unit/p7/t1.cpp" 2

export module x;

extern "C" void foo() {
  return;
}

extern "C" {
void bar() {
  return;
}
int baz() {
  return 3;
}
double double_func() {
  return 5.0;
}
}

extern "C++" {
void bar_cpp() {
  return;
}
int baz_cpp() {
  return 3;
}
double double_func_cpp() {
  return 5.0;
}
}
