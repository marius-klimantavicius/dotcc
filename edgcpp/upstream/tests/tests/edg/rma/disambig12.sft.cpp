//options_all:-r -x -tused
//options: --strict;cp

struct A { A(int=0,int=0,int=0); };
extern int i, j;
A x(int(i));
A y(int(i),0);
A yy(int(i),int(0));
A z(int(i),int(j));
A zz(int(i),int(j),int(0));

