//options_all:-r -x -tused
//options: --strict;cn

  int i = 1;
  struct D {
    int& i;
  };
  static D d1;
  D d2;
  extern D d3;
  static D d4 = { i };

