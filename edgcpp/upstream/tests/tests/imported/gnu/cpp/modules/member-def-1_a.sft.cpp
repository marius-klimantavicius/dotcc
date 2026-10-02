//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }

export module foo:part1;
// { dg-module-cmi {foo:part1} }
struct frob
{
  struct inner;
};
