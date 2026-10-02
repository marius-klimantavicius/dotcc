//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }
export module pop;
// { dg-module-cmi "pop" }
export import thing;

void bink ();

void bonk ()
{
  baz ();
  bink ();
}
