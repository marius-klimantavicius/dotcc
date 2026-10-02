//options_all:-r -x -tused
//options: --strict;cn

/* An array whose element type requires a non-default constructor is
   illegal. */
class A { int i, j; A(int); };
A a;
A b[3];
A c[] = {1,1,1};
A d[2] = {2,2};

