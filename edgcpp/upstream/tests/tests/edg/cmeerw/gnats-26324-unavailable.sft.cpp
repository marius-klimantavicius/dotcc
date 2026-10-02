//type:fn
//options:--c++11 --g++ --gnu_version 120100

struct C;

C * c1;

struct C {
  int f(C *);
} __attribute__ ((unavailable));

C *c2;                          // error: declared unavailable
