//type: fp
//options: 
# 0 "./ext/interface1a.cc"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ext/interface1a.cc"
# 1 "./ext/interface1.h" 1
#pragma interface
struct B
{
  B(){};
  ~B(){}
};
struct A {
  B a;

};
# 2 "./ext/interface1a.cc" 2
A a;
int main() {}
