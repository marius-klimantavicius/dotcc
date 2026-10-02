//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules }

import M;

int main()
{
  int *p = alloc<int>(42);
}
