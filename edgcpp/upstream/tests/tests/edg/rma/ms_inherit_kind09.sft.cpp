//options_all:-r -x -tused
//options: --microsoft -n;cp

struct A { };
struct B : A { };
struct __single_inheritance C: B { };
struct D { };
struct E : C, D { };
struct __multiple_inheritance F : E { };

