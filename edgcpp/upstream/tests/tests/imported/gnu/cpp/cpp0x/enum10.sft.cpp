//type: fp
//options: --c++11
// PR c++/48534
// { dg-do compile { target c++11 } }

enum class OpSE : bool;

int main()
{
  return static_cast<bool>(OpSE());
}
