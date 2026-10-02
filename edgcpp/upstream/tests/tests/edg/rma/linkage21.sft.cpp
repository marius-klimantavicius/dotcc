//options_all:-r -x -tused
//options: --strict;cn

static void f(int);
extern "C" void f(int);
static void g(int);
extern "C" { void g(int); }

