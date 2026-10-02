//type: fn
//options: 
# 0 "./ext/flexary5.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ext/flexary5.C"






# 1 "./ext/flexary.h" 1
# 22 "./ext/flexary.h"
typedef long unsigned int size_t;
# 8 "./ext/flexary5.C" 2

template <class T>
struct S_no_diag: T {
  char a[];
};

template <class T>
struct STx_1: T {
  char a[];
};

template <class T, int I>
struct STI: T {
  char a[I];
};

template <class T, int I>
struct STIx: T {
  char a[I];
};

template <int> struct E { };

STx_1<E<0> > stx_empty_1;
STIx<E<0>, 0> stix_empty_1;



struct E1: E<0>, E<1> { };
struct E2: E<2>, E<3> { };
struct D1: E1, E2
{
    char a[];
};

struct NE { size_t i; };

struct A1x { int n, a[]; };
struct D2: A1x, E1, E2 { };



static_assert (__builtin_offsetof (D2, a) == sizeof (D2), "__builtin_offsetof (D2, a) == sizeof (D2)");

struct D3: E1, A1x, E2 { };

static_assert (__builtin_offsetof (D3, a) == sizeof (D3), "__builtin_offsetof (D3, a) == sizeof (D3)");

struct D4: E1, E2, A1x { };

static_assert (__builtin_offsetof (D4, a) == sizeof (D4), "__builtin_offsetof (D4, a) == sizeof (D4)");




struct D5: E1, E2, NE { char a[]; };

static_assert (__builtin_offsetof (D5, a) == sizeof (D5), "__builtin_offsetof (D5, a) == sizeof (D5)");

struct A2x_1 {
  size_t n;
  size_t a[];
};

struct A2x_2 {
  size_t n;
  size_t a[];
};

struct A2x_3 {
  size_t n;
  size_t a[];
};




struct D6: A2x_1, E1, A1x { };
struct D7: E1, A2x_2, E2, A1x { };
struct D8: E1, E2, A2x_3, A1x { };

struct DA2x: A2x_1 { };

struct D9: DA2x, E1, E2 { };

static_assert (__builtin_offsetof (D9, a) == sizeof (D9), "__builtin_offsetof (D9, a) == sizeof (D9)");

struct D10: E1, DA2x, E2 { };

static_assert (__builtin_offsetof (D10, a) == sizeof (D10), "__builtin_offsetof (D10, a) == sizeof (D10)");

struct D11: E1, E2, DA2x { };

static_assert (__builtin_offsetof (D11, a) == sizeof (D11), "__builtin_offsetof (D11, a) == sizeof (D11)");

struct A3x {
  size_t n;
  size_t a[];
};




struct D12: A3x, E1, NE { };
struct D13: E1, A3x, NE { };
struct D14: E1, E2, A3x, NE { };
struct D15: E1, E2, NE, A3x { };

struct A4x {
  A4x ();
  ~A4x ();

  size_t n;
  struct AS {
    AS (int);
    ~AS ();
    size_t i;
  } a[];
};

struct D16: A4x, E1, E2 { };

static_assert (__builtin_offsetof (D16, a) == sizeof (D16), "__builtin_offsetof (D16, a) == sizeof (D16)");

struct D17: E1, A4x, E2 { };

static_assert (__builtin_offsetof (D17, a) == sizeof (D17), "__builtin_offsetof (D17, a) == sizeof (D17)");

struct D18: E1, E2, A4x { };

static_assert (__builtin_offsetof (D18, a) == sizeof (D18), "__builtin_offsetof (D18, a) == sizeof (D18)");

struct DA4x: A4x { };

struct D19: DA4x, E1, E2 { };

static_assert (__builtin_offsetof (D19, a) == sizeof (D19), "__builtin_offsetof (D19, a) == sizeof (D19)");

struct D20: E1, DA4x, E2 { };

static_assert (__builtin_offsetof (D20, a) == sizeof (D20), "__builtin_offsetof (D20, a) == sizeof (D20)");

struct D21: E1, E2, DA4x { };

static_assert (__builtin_offsetof (D21, a) == sizeof (D21), "__builtin_offsetof (D21, a) == sizeof (D21)");


struct A5x {
  A5x (int);
  virtual ~A5x ();

  size_t n;
  struct AS {
    AS (int);
    ~AS ();
    size_t i;
  } a[];
};

struct D22: A5x, E1, E2 { };

static_assert (__builtin_offsetof (D22, a) == sizeof (D22), "__builtin_offsetof (D22, a) == sizeof (D22)");

struct D23: E1, A5x, E2 { };

static_assert (__builtin_offsetof (D23, a) == sizeof (D23), "__builtin_offsetof (D23, a) == sizeof (D23)");

struct D24: E1, E2, A5x { };

static_assert (__builtin_offsetof (D24, a) == sizeof (D24), "__builtin_offsetof (D24, a) == sizeof (D24)");

struct DA5x: A5x { };

struct D25: DA5x, E1, E2 { };

static_assert (__builtin_offsetof (D25, a) == sizeof (D25), "__builtin_offsetof (D25, a) == sizeof (D25)");

struct D26: E1, DA5x, E2 { };

static_assert (__builtin_offsetof (D26, a) == sizeof (D26), "__builtin_offsetof (D26, a) == sizeof (D26)");

struct D27: E1, E2, DA5x { };

static_assert (__builtin_offsetof (D27, a) == sizeof (D27), "__builtin_offsetof (D27, a) == sizeof (D27)");



struct A6x {
  size_t n;
  size_t a[];
};

struct AA6x: A6x { };
struct NE1: NE { };
struct NE2: NE { };

struct D28: NE1, AA6x { };
struct D29: AA6x, NE1 { };

struct A7x {
  size_t n;
  size_t a[];
};



struct DA7xV1: virtual A7x { };
struct DA7xV2: virtual A7x { };

struct D30: DA7xV1, DA7xV2 { };
struct D31: DA7xV1, DA7xV2 { };
struct D32: D30, D31 { };


struct A8x {
  struct {
    size_t n;
    size_t a[];
  };
};

struct D33:
  A7x, A8x { };
