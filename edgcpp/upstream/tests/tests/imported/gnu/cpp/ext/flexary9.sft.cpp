//type: fn
//options: 
# 0 "./ext/flexary9.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ext/flexary9.C"



# 1 "./ext/flexary.h" 1
# 22 "./ext/flexary.h"
typedef long unsigned int size_t;
# 5 "./ext/flexary9.C" 2

struct Sx {
  int a[0];
};




struct Sx2 {
  int a[0];
  typedef int I;
};

struct Sx3 {
  typedef int I;
  int a[0];
};

struct Sx4 {
  int a[0];
  enum E { e };
};

struct Sx5 {
  enum E { e };
  int a[0];
};

struct Sx6 {
  int a[0];
  static int i;
};

struct Sx7 {
  static int i;
  int a[0];
};

struct Sx8 {
  int a[0];
  Sx8 () { }
};

struct Sx9 {
  Sx9 () { }
  int a[0];
};

struct Sx10 {
  int a[0];
  virtual ~Sx10 () { }
};

struct Sx11 {
  virtual ~Sx11 () { }
  int a[0];
};

struct Sx12 {
  int a[0];
  virtual void foo () = 0;
};

struct Sx13 {
  virtual void foo () = 0;
  int a[0];
};

struct Sx14 {
  int a[0][1];
};

struct Sx15 {
  typedef int A[0];
  A a;
};


struct Sx16 {
  int a_0 [0];
  int a_x [0];
};

struct Sx17 {
  int a_x [0];
  int a_0 [0];
};






struct Sx18 {
  int a_x [0];
  struct S { };
};





struct Sx19 {
  struct S { };
  union U { };
  int a_x [0];
};



struct Sx20 {
  struct S { } s;
  int a_x [0];
};

struct Sx21 {
  int a_x [0];
  struct S { } s;
};

struct Sx22 {
  int a_x [0];
  union { int i; };
};

struct Sx23 {
  union { int i; };
  int a_x [0];
};



struct Sx24 {
  struct S;
  S a_x [0];

};

struct Sx25 {
  struct S { };
  S a_x [0];
};

struct Sx26 {
  struct { }
    a_x [0];
};

struct Sx27 {
  int i;
  struct { }
    a_x [0];
};

static_assert (__builtin_offsetof (Sx27, a_x) == sizeof (Sx27), "__builtin_offsetof (Sx27, a_x) == sizeof (Sx27)");

struct Sx28 {
  struct { }
    a_x [0];
  int i;
};

struct Sx29 {

  int (*a_x)[0];
};

struct Sx30 {

  int (&a_x)[0];
};

struct Sx31 {
  int a [0];
  unsigned i: 1;
};

struct Sx32 {
  unsigned i: 1;
  int a [0];
};

static_assert (__builtin_offsetof (Sx32, a) == sizeof (Sx32), "__builtin_offsetof (Sx32, a) == sizeof (Sx32)");

struct Sx33 {
  int a [0];
  friend int foo ();
};

struct Sx34 {
  friend int foo ();
  int a [0];
};



struct Sx35 {
  int a[0];
  typedef int I;
  int n;
};

struct Sx36 {
  int n;
  typedef int I;
  int a[0];
};

static_assert (__builtin_offsetof (Sx36, a) == sizeof (Sx36), "__builtin_offsetof (Sx36, a) == sizeof (Sx36)");

struct Sx37 {
  int a[0];
  enum E { };
  int n;
};

struct Sx38 {
  int n;
  enum E { };
  int a[0];
};

static_assert (__builtin_offsetof (Sx38, a) == sizeof (Sx38), "__builtin_offsetof (Sx38, a) == sizeof (Sx38)");

struct Sx39 {
  int a[0];
  struct S;
  int n;
};

struct Sx40 {
  int n;
  struct S;
  int a[0];
};

static_assert (__builtin_offsetof (Sx40, a) == sizeof (Sx40), "__builtin_offsetof (Sx40, a) == sizeof (Sx40)");

struct Sx41 {
  int a[0];
  static int i;
  int n;
};

struct Sx42 {
  int n;
  static int i;
  int a[0];
};

static_assert (__builtin_offsetof (Sx42, a) == sizeof (Sx42), "__builtin_offsetof (Sx42, a) == sizeof (Sx42)");

struct Sx43 {
  int a[0];
  Sx43 ();
  int n;
};

struct Sx44 {
  int n;
  Sx44 ();
  int a[0];
};

static_assert (__builtin_offsetof (Sx44, a) == sizeof (Sx44), "__builtin_offsetof (Sx44, a) == sizeof (Sx44)");

struct S_S_S_x {
  struct A {
    struct B {
      int a[0];
    } b;
  } a;
};





struct Anon1 {
  int n;
  struct {
    int good[0];
  };
};

static_assert (__builtin_offsetof (Anon1, good) == sizeof (Anon1), "__builtin_offsetof (Anon1, good) == sizeof (Anon1)");

struct Anon2 {
  struct {
    int n;
    struct {
      int good[0];
    };
  };
};

static_assert (__builtin_offsetof (Anon2, good) == sizeof (Anon2), "__builtin_offsetof (Anon2, good) == sizeof (Anon2)");

struct Anon3 {
  struct {
    struct {
      int n;
      int good[0];
    };
  };
};

static_assert (__builtin_offsetof (Anon3, good) == sizeof (Anon3), "__builtin_offsetof (Anon3, good) == sizeof (Anon3)");

struct Anon4 {
  struct {
    int in_empty_struct[0];
  };
};

struct Anon5 {
  struct {
    int not_at_end[0];
  };
  int n;
};

struct Anon6 {
  struct {
    struct {
      int not_at_end[0];
    };
    int n;
  };
};


struct Anon7 {
  struct {
    struct {
      int not_at_end[0];
    };
  };
  int n;
};


struct Six {
  int i;
  int a[0];
};

static_assert (__builtin_offsetof (Six, a) == sizeof (Six), "__builtin_offsetof (Six, a) == sizeof (Six)");

class Cx {
  int a[0];
};

class Cix {
  int i;
  int a[0];
};

struct Sxi {
  int a[0];
  int i;
};

struct S0 {
  int a[0];
};

struct S0i {
  int a[0];
  int i;
};

struct S_a0_ax {
  int a1[0];
  int ax[0];
};

struct S_a0_i_ax {
  int a1[0];
  int i;
  int ax[0];
};

static_assert (__builtin_offsetof (S_a0_i_ax, ax) == sizeof (S_a0_i_ax), "__builtin_offsetof (S_a0_i_ax, ax) == sizeof (S_a0_i_ax)");

struct Si_a0_ax {
  int i;
  int a1[0];
  int ax[0];
};

static_assert (__builtin_offsetof (Si_a0_ax, ax) == sizeof (Si_a0_ax), "__builtin_offsetof (Si_a0_ax, ax) == sizeof (Si_a0_ax)");

struct S_u0_ax {
  union { } u[0];
  int ax[0];
};

struct S_a1_s2 {
  int a[1];
  int b[2];
};
