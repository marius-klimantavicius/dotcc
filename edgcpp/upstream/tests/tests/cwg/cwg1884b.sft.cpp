//type:fn
//options_all:--c++20 -tused -A
static void f();
extern "C" void h();
static int i = 0;               // #1
void q() {
  extern void f();              // internal linkage
  extern void g();              // ::g, external linkage
  extern void h();              // C language linkage
  int i;                        // #2: i has no linkage
  {
    extern void f();            // internal linkage
    extern int i;               // #3: internal linkage, ill-formed
  }
}
