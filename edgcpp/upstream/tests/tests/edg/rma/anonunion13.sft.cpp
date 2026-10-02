//options_all:-r -x -tused
//options: --strict;cn

typedef struct _A {
  int i, j;
} A;
typedef struct _B {
  union _Bu {
    struct _Bs {
      A;
    };
  };
} B;
struct _C {
  union _Cu {
    struct _Cs {
      B;
    };
  };
} x;
struct _D {
  union _Du {
    struct _Ds {
      B;
    };
  };
} y;
int main() {
  x.i = 0;
  y.i = 0;
}

