//options_all:-r -x -tused
//options: --strict;cn:;cn

struct S {
  static union { int X; };
  int arr1[sizeof(X)];
  static int Y;
  int arr2[sizeof(Y)];
};

