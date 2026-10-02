//options_all:-r -x -tused
//options: --strict;cn

class A { virtual int f() = 0; };
typedef A Atype[2];
A a;             // abstract class (illegal)
A b[2];          // array of abstract class (illegal)
A *c[3];         // array of pointer to abstract class
A (*d)[4];       // pointer to array of abstract class (illegal)
Atype x;         // array of abstract class (illegal)
Atype y[2];      // array of array of abstract class (illegal)
Atype *z;        // pointer to array of abstract class (illegal)
Atype* f(        // returns pointer to array of abstract class (illegal)
   Atype,        // arg array of abstract class (= ptr to abstract class?)
   A[2],         // arg array of abstract class (= ptr to abstract class?)
   Atype*);      // arg pointer to array of abstract class

