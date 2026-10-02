//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cn:;cp

int f() { return 0; };
class A { friend int f(int i) { return i; } };

