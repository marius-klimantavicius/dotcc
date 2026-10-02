//options_all:-r -x -tused
//options: --strict;cn

  struct C {
    const int i;
  };
  static C c1;
  static const C c2;
  C c3;
  extern C c4;
  static C c5 = { 1 };

