//options_all:-r -x -tused
//options: --strict;cn

struct B {
  // try-block
  B(int) {
    try {
L:;
    } catch (...) {
      goto L;
    }
  }

  // function-try-block
  B() try {
L:;
  } catch (...) {
    goto L;
  }
};

