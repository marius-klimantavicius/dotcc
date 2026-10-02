//options_all:-r -x -tused
//options: --strict;cn

  class A { A(int,int); };
  extern int i;
  A a(int(i),int(j));       // function declaration
  A b(int(i),int(1));       // variable declaration with ctor initializer
  A c(int(1),int(i));       // variable declaration with ctor initializer


