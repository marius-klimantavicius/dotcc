//options_all:-r -x -tused
//options: --strict;cp

void f(int,int,int,int=0);
void f(int,int,int=0,int);
void f(int,int=0,int,int) { }
void f(int=0,int,int,int);
void f(int,int,int,int);

