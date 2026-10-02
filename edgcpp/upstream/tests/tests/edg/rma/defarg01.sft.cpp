//options_all:-r -x -tused
//options: --strict;cn:;cn

int f(int i = 0) { return i; }
int f() { return -1; }
main() { (void)f(); }

