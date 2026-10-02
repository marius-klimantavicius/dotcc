//options_all:-r -x -tused
//options: --strict;cp

struct a {
  a() { }
  a(int)  { }
};

struct aa {
  aa(int)  { }
};

struct b : a {
  aa a;
  b(int i) : a(2*i) { /* a::a(i);*/ }  // initialize b::a or call a::a() ?
};

b binst(5);

