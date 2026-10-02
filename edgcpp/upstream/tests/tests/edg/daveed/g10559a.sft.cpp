//remark:Microsoft __builtinalignof
//options:--c;fp:--c++;fp
//options_all:--microsoft

struct S { int x; } s;

int main() {
  return __builtin_alignof(struct S) + __builtin_alignof(s);
}

