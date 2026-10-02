//options_all:-r -x -tused
//options: --strict;cn:;cp

// Local classes have no linkage (neither external nor internal)
struct A {char c;} v;
main () {
  { struct A {int i;} v;
    v.i = 1;
  }
  { struct A {float j;} v;
    v.j = 1.0;
  }
  v.c = 'a';
}

