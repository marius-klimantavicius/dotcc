//type:cp
//options::-DNEG;fn
//options_all:--c++20

double a[](1,2,3);
double* p = new double[](1,2,3);

int ia[](1);
int *ip = new int[](1);
#if NEG
int ib[1](1,2,3);
int *ipb = new int[1](1,2,3);
#endif
