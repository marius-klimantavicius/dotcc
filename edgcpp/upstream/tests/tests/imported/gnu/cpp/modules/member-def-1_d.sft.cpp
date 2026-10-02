//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts -fdump-lang-module" }

module foo;

void f ()
{
  frob::inner x;
  x.i = 17;
}

// { dg-final { scan-lang-dump {Loaded 1 clusters} module } }
