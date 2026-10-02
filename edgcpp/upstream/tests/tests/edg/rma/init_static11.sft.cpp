//options_all:-r -x -tused
//options: --strict;cp

int f();
static int i, j=i, k=f();
struct S { S(); S(int); static S f(); int i; };
static S a, b=a, c(0), d=0, e=f(), g=::f();

