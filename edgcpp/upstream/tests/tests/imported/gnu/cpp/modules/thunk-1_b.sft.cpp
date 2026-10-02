//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }
export module baz;
// { dg-module-cmi baz }

import foo;

export struct Container  : virtual Derived
{
  Container () {}
  ~Container () {}
};
  
  
  
