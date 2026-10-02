//options_all:-r -x -tused
//options: --strict;cp

struct S {
  int x;
  S();// : x(0) { }
  S(int i);// : x(i) { }
};
void f() {
  static int i = 1;
  static int j[2] = { 2, 2 };
  static int k = i;
  static S s1;
  static S s2(3);
  static S s3 = s1;
}

