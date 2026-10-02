//options_all:-r -x -tused
//options: --strict;cn

extern int a[];
int a[3];
extern int (*p)[];
int (*p)[3];                 // Incompatibility error
extern int (&r)[];
int (&r)[3] = a;             // Incompatibility error
extern int a2[3];
int a2[];
extern int (*p2)[3];
int (*p2)[];                 // Incompatibility error
extern int (&r2)[3];
int (&r2)[] = a2;            // Incompatibility error
class A {
  static int a[];
  static int (*p)[];
  static int (&r)[];
};
int A::a[3];
int (*A::p)[3];              // Incompatibility error
int (&A::r)[3] = A::a;       // Incompatibility error
class B {
  static int a[3];
  static int (*p)[3];
  static int (&r)[3];
};
int B::a[];
int (*B::p)[];              // Incompatibility error
int (&B::r)[] = B::a;       // Incompatibility error

