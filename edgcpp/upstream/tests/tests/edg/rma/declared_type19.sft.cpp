//options_all:-r -x -tused
//options: --strict;cp

// EDGqa01368
struct S {
  inline S(char * = "abc") { }
  void f(int i = 0) { }
};
S s;

