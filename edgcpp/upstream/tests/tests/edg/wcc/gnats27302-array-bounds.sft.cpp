//type:fp
//options_all:--c++11

struct A { ~A() {} };

A y[~((unsigned long)0)];
