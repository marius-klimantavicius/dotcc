//options_all:-r -x -tused
//options: --microsoft -n;cp

struct A;
struct X {
  typedef A B;
};
struct Y {
  typedef A B;
};
struct A : X, Y {
  B();               // ambiguity error
};

