//type: fp
//options: 
# 1 "SemaCXX/warn-sysheader-macro.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 482 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/warn-sysheader-macro.cpp" 2
# 23 "SemaCXX/warn-sysheader-macro.cpp"
# 1 "SemaCXX/warn-sysheader-macro.cpp" 1
# 8 "SemaCXX/warn-sysheader-macro.cpp" 3







struct Foo {
  int x;
};
# 24 "SemaCXX/warn-sysheader-macro.cpp" 2

void testSanity() {

  int i = (0 / 0);
}

void PR16093() {

  int i = __extension__({ int v = __extension__({ int v = 1; v; }); v; });
}

void PR18147() {

  int i = ((int) (0));
}

void PR52944() {

  auto i = (Foo{.x = 123});
}
