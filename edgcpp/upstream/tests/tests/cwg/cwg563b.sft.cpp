//type:fp
//options_all:--c++20 -tused -A 

extern "C" typedef void FUNC_c();

class C {
  void mf1(FUNC_c*);            // the function mf1 and its type have C++ language linkage;
                                // the parameter has type "pointer to C function"

  FUNC_c mf2;                   // the function mf2 and its type have C++ language linkage

  static FUNC_c* q;             // the data member q has C++ language linkage;
                                // its type is "pointer to C function"
};

extern "C" {
  class X {
    void mf();                  // mf and its type have C++ language linkage
    void mf2(void(*)());        // the function mf2 has C++ language linkage;
                                // the parameter has type "pointer to C function"
  };
}
