//type:fp
//options_all:--c++17 -tused -A
  struct A { int n; } a; 
  int *p = reinterpret_cast<int*>(&a); // ok, a and a.n are pointer-interconvertible
  int m = *p;                          // ok, p points to a.n
  int &r = reinterpret_cast<int&>(a); 
  int n = r; 

//cwg: 2342
//title: Reference reinterpret_cast and pointer-interconvertibility
//meeting: Albuquerque 11/17
//edg_status: Passes
