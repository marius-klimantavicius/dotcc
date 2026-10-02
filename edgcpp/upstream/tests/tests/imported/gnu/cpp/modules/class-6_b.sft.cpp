//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }
module One;

pad::~pad ()
{
}

int base::getter () const
{
  return b;
}
