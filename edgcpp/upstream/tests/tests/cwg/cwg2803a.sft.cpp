//options_all:--c++23 -tused -A
  int f(const volatile int *);
  int f(const int *);
  int i;
  int j = f(&i);  // calls f(const int*)
  int g(const int*);
  int g(const volatile int* const&);
  int* p;
  int k = g(p);          // calls g(const int*)

//cwg: 2803
//title: Overload resolution for reference binding of similar types
//meeting: Tokyo 3/24
//edg_status: EDgcpfe/27136
