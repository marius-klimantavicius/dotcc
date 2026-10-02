//options_all:-r -x -tused
//options: --microsoft -n;cn

struct A { };
struct B : A { };
struct C { };
struct D : A, C { };
struct E : D { };
struct __single_inheritance X : B { };  // okay
struct __single_inheritance Y : D { };  // error
struct __single_inheritance Z : E { };  // error

