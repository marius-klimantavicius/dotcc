//options_all:-r -x -tused
//options: --strict;cp:;cp

class C {
  class X {};
  class Y {};
  class N : X {
    X *p;             // Okay -- it's a base class
    Y *q;             // Used to be an accessibility error in strict mode
  };
};

