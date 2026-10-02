//options_all:-r -x -tused
//options: --strict;cp

struct S { int a,b,c; };
void f(void) {
struct S s = {1, 2, 3};
struct S t = s;
}

