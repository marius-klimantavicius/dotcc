//type: fn
//options: 
# 0 "./ext/flexary4.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ext/flexary4.C"
# 11 "./ext/flexary4.C"
# 1 "./ext/flexary.h" 1
# 22 "./ext/flexary.h"
typedef long unsigned int size_t;
# 12 "./ext/flexary4.C" 2

struct Sx {
  int a[];
};




struct Sx2 {
  int a[];
  typedef int I;
};

struct Sx3 {
  typedef int I;
  int a[];
};

struct Sx4 {
  int a[];
  enum E { e };
};

struct Sx5 {
  enum E { e };
  int a[];
};

struct Sx6 {
  int a[];
  static int i;
};

struct Sx7 {
  static int i;
  int a[];
};

struct Sx8 {
  int a[];
  Sx8 () { }
};

struct Sx9 {
  Sx9 () { }
  int a[];
};

struct Sx10 {
  int a[];
  virtual ~Sx10 () { }
};

struct Sx11 {
  virtual ~Sx11 () { }
  int a[];
};

struct Sx12 {
  int a[];
  virtual void foo () = 0;
};

struct Sx13 {
  virtual void foo () = 0;
  int a[];
};

struct Sx14 {
  int a[][1];
};

struct Sx15 {
  typedef int A[];
  A a;
};


struct Sx16 {


  int a_0 [0];
  int a_x [];
};

struct Sx17 {
  int a_x [];



  int a_0 [0];
};






struct Sx18 {
  int a_x [];
  struct { } s;
};



struct Sx19 {
  struct { int i; };
  int a_x [];
};



struct Sx20 {
  struct S { int i; };
  int a_x [];
};

struct Sx21 {
  int a_x [];
  struct S { } s;
};

struct Sx22 {
  int a_x [];
  union { int i; };
};

struct Sx23 {
  union { int i; };
  int a_x [];
};

struct Sx24 {
  struct S;
  S a_x [];
};

struct Sx25 {
  struct S { };
  S a_x [];
};

struct Sx26 {
  struct { }
    a_x [];
};

struct Sx27 {
  int i;
  struct { }
    a_x [];
};

static_assert (__builtin_offsetof (Sx27, a_x) == sizeof (Sx27), "__builtin_offsetof (Sx27, a_x) == sizeof (Sx27)");

struct Sx28 {
  struct { }
    a_x [];
  int i;
};

struct Sx29 {

  int (*a_x)[];
};

struct Sx30 {

  int (&a_x)[];
};

struct Sx31 {
  int a [];
  unsigned i: 1;
};

struct Sx32 {
  unsigned i: 1;
  int a [];
};

static_assert (__builtin_offsetof (Sx32, a) == sizeof (Sx32), "__builtin_offsetof (Sx32, a) == sizeof (Sx32)");

struct Sx33 {
  int a [];
  friend int foo ();
};

struct Sx34 {
  friend int foo ();
  int a [];
};



struct Sx35 {
  int a[];
  typedef int I;
  int n;
};

struct Sx36 {
  int n;
  typedef int I;
  int a[];
};

static_assert (__builtin_offsetof (Sx36, a) == sizeof (Sx36), "__builtin_offsetof (Sx36, a) == sizeof (Sx36)");

struct Sx37 {
  int a[];
  enum E { };
  int n;
};

struct Sx38 {
  int n;
  enum E { };
  int a[];
};

static_assert (__builtin_offsetof (Sx38, a) == sizeof (Sx38), "__builtin_offsetof (Sx38, a) == sizeof (Sx38)");

struct Sx39 {
  int a[];
  struct S;
  int n;
};

struct Sx40 {
  int n;
  struct S;
  int a[];
};

static_assert (__builtin_offsetof (Sx40, a) == sizeof (Sx40), "__builtin_offsetof (Sx40, a) == sizeof (Sx40)");

struct Sx41 {
  int a[];
  static int i;
  int n;
};

struct Sx42 {
  int n;
  static int i;
  int a[];
};

static_assert (__builtin_offsetof (Sx42, a) == sizeof (Sx42), "__builtin_offsetof (Sx42, a) == sizeof (Sx42)");

struct Sx43 {
  int a[];
  Sx43 ();
  int n;
};

struct Sx44 {
  int n;
  Sx44 ();
  int a[];
};

static_assert (__builtin_offsetof (Sx44, a) == sizeof (Sx44), "__builtin_offsetof (Sx44, a) == sizeof (Sx44)");

struct S_S_S_x {
  struct A {
    struct B {
      int a[];
    } b;
  } a;
};





struct Anon1 {
  int n;
  struct {
    int good[];
  };
};

static_assert (__builtin_offsetof (Anon1, good) == sizeof (Anon1), "__builtin_offsetof (Anon1, good) == sizeof (Anon1)");

struct NotAnon1 {
  int n;


  struct {
    int bad[];
  } name;
};

struct Anon2 {
  struct {
    int n;
    struct {
      int good[];
    };
  };
};

static_assert (__builtin_offsetof (Anon2, good) == sizeof (Anon2), "__builtin_offsetof (Anon2, good) == sizeof (Anon2)");

struct Anon3 {
  struct {
    struct {
      int n;
      int good[];
    };
  };
};

static_assert (__builtin_offsetof (Anon3, good) == sizeof (Anon3), "__builtin_offsetof (Anon3, good) == sizeof (Anon3)");

struct Anon4 {
  struct {
    int in_empty_struct[];
  };
};

struct Anon5 {
  struct {
    int not_at_end[];
  };
  int n;
};

struct Anon6 {
  struct {
    struct {
      int not_at_end[];
    };
    int n;
  };
};


struct Anon7 {
  struct {
    struct {
      int not_at_end[];
    };
  };
  int n;
};

struct Six {
  int i;
  int a[];
};

static_assert (__builtin_offsetof (Six, a) == sizeof (Six), "__builtin_offsetof (Six, a) == sizeof (Six)");

class Cx {
  int a[];
};

class Cix {
  int i;
  int a[];
};

struct Sxi {
  int a[];
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
  int a0[0];
  int ax[];
};

struct S_a0_i_ax {
  int a0[0];
  int i;
  int ax[];
};

static_assert (__builtin_offsetof (S_a0_i_ax, ax) == sizeof (S_a0_i_ax), "__builtin_offsetof (S_a0_i_ax, ax) == sizeof (S_a0_i_ax)");

struct Si_a0_ax {
  int i;
  int a0[0];
  int ax[];
};

static_assert (__builtin_offsetof (Si_a0_ax, ax) == sizeof (Si_a0_ax), "__builtin_offsetof (Si_a0_ax, ax) == sizeof (Si_a0_ax)");

struct Si_ax_a0 {
  int i;
  int ax[];
  int a0[0];
};

struct S_u0_ax {
  union { } u[0];
  int ax[];
};

struct S_a1_s2 {
  int a[1];
  int b[2];
};
