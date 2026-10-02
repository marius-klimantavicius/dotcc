//type: fp
//options:  --c++20 --modules
// { dg-module-do link }
// { dg-additional-options -fmodules-ts }

export module foo:part1;
// { dg-module-cmi {foo:part1} }

export struct frob 
{
  void member ();
};
