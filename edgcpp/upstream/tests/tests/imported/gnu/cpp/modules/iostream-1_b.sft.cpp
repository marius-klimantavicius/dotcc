//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }

import "iostream-1_a.H";

int main ()
{
  std::cout << "hello world!\n";
  return 0;
}
