//remark:GNU constant folding
//options:--gcc;fp:--gcc -DNEG;fn:--c99;fn

void a() {
  int c = 10;
#ifdef NEG
  double x[c == c ? -1 : 1];
#endif
  int b = sizeof(struct { int a : c == c; });
}
