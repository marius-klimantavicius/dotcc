//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }

import std;

int main ()
{
  return !std::frob ();
}
