//options_all:-r -x -tused
//options: --strict;cp

int f();
int f(int);
class A { friend int f(); friend int f(int); friend int f(float); };

