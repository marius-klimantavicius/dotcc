//type: fn
//options_all: -A --c++20 -tused -e 200 --no_wrap
//
  static void f();
  extern "C" void h();
  static int i = 0;      // #1
  void g() {
    extern void f();     // internal linkage
    extern void h();     // C language linkage

    int i;               // #2: i has no linkage
    {
      extern void f();   // internal linkage
      extern int i;      // #3: external linkage, ill-formed
    }
  }

//cwg: 2372
//title: Incorrect matching rules for block-scope extern declarations
//meeting: Kona 02/19
//edg_status: Passes
