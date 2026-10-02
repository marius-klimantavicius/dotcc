//options_all:-r -x -tused
//options: --strict;cn:;cn

#ifdef PROTO
void foo(struct S { int i; } *ps) { }
#else
void foo(ps) struct S { int i; } *ps; { }
#endif
struct S s;
main() {
  foo(&s);
}

