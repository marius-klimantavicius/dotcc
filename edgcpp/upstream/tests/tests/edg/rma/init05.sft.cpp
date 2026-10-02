//options_all:-r -x -tused
//options: --strict;cp

class A { static int a, b; };
int A::a = 0;
int A::b = a;
int a = 0;
int b = a;

