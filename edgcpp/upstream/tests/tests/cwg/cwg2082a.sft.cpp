//type:fp
//options_all:--c++17 -tused -A
//
  void f() {
    int i;
    extern void h(int x = sizeof(i));   // OK
    // ...
  }

//cwg: 2082
//title: Referring to parameters in unevaluated operands of default arguments
//meeting: Jacksonville 2/16
//edg_status: EDGcpfe/21996
