//options_all:-r -x -tused
//options: --cfront_3.0;cp:;cn

// Okay in cfront mode
typedef struct { int i; } S;
struct A {
  friend struct S;
};
A a;


