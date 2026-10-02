//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts -fdump-lang-module-uid" }

import bob;

int a = foo ();

// { dg-final { scan-lang-dump {Binding of '::foo'} module } }
