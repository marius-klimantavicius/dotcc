//type: rp
//options: --c++20 lambda-uneval9.cc
# 0 "./cpp2a/lambda-uneval9.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp2a/lambda-uneval9.C"



# 1 "./cpp2a/lambda-uneval9.h" 1

template <typename T>
int counter() {
  static int cnt = 0;
  return ++cnt;
}
inline int f() {
  return counter<decltype([] {})>();
}
# 5 "./cpp2a/lambda-uneval9.C" 2
int foo() { return f(); }
extern int bar();

int main()
{
  if (foo() != 1) __builtin_abort();
  if (bar() != 2) __builtin_abort();
}
