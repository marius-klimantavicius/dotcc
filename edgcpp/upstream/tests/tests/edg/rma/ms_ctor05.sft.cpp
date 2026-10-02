//options_all:-r -x -tused -n --diag_error 837
//options:--microsoft_version=1200;cp:--microsoft_version=1310;cn

struct A;
typedef A B;
struct X {
  int B;
};
struct A : X {
  B();
};
//A::B() { }


