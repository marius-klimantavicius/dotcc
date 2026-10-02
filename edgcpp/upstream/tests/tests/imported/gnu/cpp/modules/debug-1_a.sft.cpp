//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts -g" }

export module frob;
// { dg-module-cmi frob }

export struct thingy
{
  virtual void X () 
  {
    thingy w;
  }
};
