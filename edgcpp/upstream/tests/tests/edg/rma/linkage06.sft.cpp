//options_all:-r -x -tused
//options: --cfront_3.0;cp

extern "C" void f1();
extern "C" void f2();
extern "C" {
extern "C" void g1() {
  extern void f1();      // edg: C linkage;  cfront: C++ linkage, warning
  extern void f3();      // edg: C linkage;  cfront: C++ linkage
}
}
extern "C" {
void g2() {
  extern void f2();      // edg: C linkage;  cfront: C linkage
  extern void f4();      // edg: C linakge;  cfront: C linakge
}
}
extern "C" void f3();    // cfront 3.0 error
extern "C" void f4();

