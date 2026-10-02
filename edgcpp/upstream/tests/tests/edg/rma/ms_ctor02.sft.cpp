//options_all:-r -x -tused
//options: --microsoft -n;cp

struct A;
typedef A B;
struct A {
  B();
};


