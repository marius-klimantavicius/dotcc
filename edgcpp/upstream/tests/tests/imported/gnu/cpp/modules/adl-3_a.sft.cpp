//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }
export module worker;
// { dg-module-cmi worker }

namespace details {

int fn (int x)
{
  return x;
}

}
