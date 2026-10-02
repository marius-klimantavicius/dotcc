//type:cp
//options_all:--c++11 --g++

struct C
{
  C() : b{N2()} {}

  struct N1 {
    ~N1();
  };

  struct N2 {
    ~N2();
  };

  N1 a[2];
  N2 b[2];
};
