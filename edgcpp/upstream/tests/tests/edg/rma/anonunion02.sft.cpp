//options_all:-r -x -tused
//options: --strict;cp

class C {
  union {
    int i;
    double d;
  };
  C(int ii) : i(ii) { };
};


