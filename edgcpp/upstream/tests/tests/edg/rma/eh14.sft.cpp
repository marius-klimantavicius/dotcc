//options_all:-r -x -tused
//options: --strict;cn

typedef void F() throw();
F f;
void f() throw(int);
F g;
void g() throw();
F h;
void h();

