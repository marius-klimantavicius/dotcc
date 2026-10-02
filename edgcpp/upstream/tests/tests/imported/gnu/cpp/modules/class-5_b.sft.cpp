//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }
module One;

int base::getter () const
{
  return b;
}
