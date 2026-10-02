//type:fp
//options_all:--c++20 -tused -A
static void f();
static int i = 0; // #1
void g() {
  extern void f(); // internal linkage
  extern void h(); // ::h, external linkage
  int i; // #2: i has no linkage
  {
    extern void f(); // internal linkage
    extern int i; // #3: internal linkage
  }
}

//cwg: 1839
//title: Lookup of block-scope extern declarations
//meeting: Virtual 11/20*
//edg_status: EDGcpfe/23826
