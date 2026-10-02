//type: rp
//options:  interface1-a.cc
# 0 "./opt/interface1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./opt/interface1.C"




#pragma implementation "interface1.h"

# 1 "./opt/interface1.h" 1
#pragma interface "interface1.h"

struct Test {
  void f();
};

inline void Test::f() {
}
# 8 "./opt/interface1.C" 2

extern void g();

int main () {
  g();
}
