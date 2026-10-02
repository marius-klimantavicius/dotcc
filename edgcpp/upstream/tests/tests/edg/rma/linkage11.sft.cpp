//options_all:-r -x -tused
//options: --strict;cp

extern "C" {
  class A {
    typedef void F();              // C function type
    typedef F *PF;                 // pointer to C function type
    F f;                           // C++ name f; C++ function
    void f(PF);                    // C++ name f; C++ function taking
                                   //   param pointer to C function
  };
}

