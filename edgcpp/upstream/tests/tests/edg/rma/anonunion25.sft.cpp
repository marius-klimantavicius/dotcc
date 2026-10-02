//options_all:-r -x -tused
//options: --strict;ln

struct S {
  union {
    const int i, j;
  };
};// s1, s2;
//main() {
//  s1 = s2;
//}

