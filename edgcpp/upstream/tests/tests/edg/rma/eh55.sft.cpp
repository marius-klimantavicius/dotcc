//options_all:-r -x -tused
//options: --strict;cn

struct S {
  S() try { } catch(...) { return; }
  S(int i) try { } catch(...) { if (i>0) { return; } }
};

