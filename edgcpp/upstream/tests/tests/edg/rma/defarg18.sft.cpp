//options_all:-r -x -tused
//options: --strict;cn

// EDGqa00614

// [jsa: The issue here is when the default arguments are processed and
// therefore when the constructor is seen to be a default constructor.  I
// had discussed with Mike the idea of maintaining a list of default
// argument expressions in order of source appearance, including nested
// classes (i.e., maintaining such a list only for non-nested classes, and
// placing the fixup entries for the nested classes on the surrounding
// class's list), and then doing the fixup in that order.]

struct X {
  struct N {
    N(int i = 0) { }
  };
  X (N n = N()) { }
};

struct Y {
  struct N {
    N(Y y = Y()) { }
  };
  Y (int i = 0) { }
};


