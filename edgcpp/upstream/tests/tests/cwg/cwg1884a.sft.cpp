//type:fp
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
  }
}

//cwg: 1884
//title: Unclear requirements for same-named external-linkage entities
//meeting: Virtual 11/20*
//edg_status: Passes
