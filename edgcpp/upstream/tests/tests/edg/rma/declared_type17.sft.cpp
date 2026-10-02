//options_all:-r -x -tused --diag_warning=949
//options: --strict;cn:;cp

//type:cp
// cp_gen_be test: default arguments.
void f(int , int , int = 1);
void f(int , int  = 2, int );
void f(int  = 1, int , int ) {};
typedef void F(int, int =2);
F g;
void g(int =1, int) {}
void (*p)(int, int n = 2);


