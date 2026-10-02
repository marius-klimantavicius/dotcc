//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }
export module Kevin;
// { dg-module-cmi Kevin }

import Bob;

export int kevin ();
