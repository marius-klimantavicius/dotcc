//type: fp
//options: --c++11
// { dg-do compile { target c++11 } }

namespace std
{
  typedef int I;
}

void foo ()
{
  using namespace std;

  auto l = [] (I) {};
}
