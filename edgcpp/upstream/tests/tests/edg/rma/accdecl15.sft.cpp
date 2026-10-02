//options_all:-r -x -tused
//options: --strict;cn

struct A {
  int i, ii;
  typedef struct s { int m; } t;
  typedef struct ss { int m; } tt;
};

struct D : private A {
  A::s;
  struct s {};  /* error */
  struct ss {};
  A::ss;        /* error */
};
struct E : private A {
  A::t;
  int t;        /* error */
  int tt;
  A::tt;        /* error */
};
struct F : private A {
  A::t;
  struct t {};  /* error */
  struct tt {};
  A::tt;        /* error */
};


