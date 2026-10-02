//options_all:-r -x -tused
//options: --strict;cn

struct X {
  static X xx;
  X(int i);
  X(const X&, int i = (throw xx, 1)) { }
};


