//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }
module bob;

void Foo () 
{
  Bar ();
  Baz ();
}

extern "C++" void Bar ();
extern "C++" void Baz ();

