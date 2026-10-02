//options_all:-r -x -tused
//options: --strict;cn

int f(int, int);
int f(int i, int j = 0) { return i+j; }
void g(int i, int j) { (void)f(i,j); (void)f(i); (void)f(); }
int f(int i = 1, int);
void h(int i, int j) { (void)f(i,j); (void)f(i); (void)f(); }

