//options_all:-r -x -tused
//options: --strict;cn:;rp

void f1(int=0) throw(int);
void f1(int) throw(int){}
void f2(int=0) throw(int);
void f2(int) throw(int);
void f3(int) throw(int);
void f3(int=0) throw(int);
void f4(int) throw(int){}
void f4(int=0) throw(int);
main() {
  extern void f1(int) throw(int);
  extern void f2(int) throw(int);
  extern void f3(int) throw(int);
  extern void f4(int) throw(int);
  extern void f5(int) throw(int);
  extern void f6(int) throw(int);
  extern void f7(int) throw(int);
  extern void f8(int) throw(int);
}
void f1(int) throw(int);
void f2(int) throw(int);
void f3(int) throw(int);
void f4(int) throw(int);
void f5(int) throw(int);
void f6(int) throw(int);
void f7(int) throw(int);
void f8(int) throw(int);




