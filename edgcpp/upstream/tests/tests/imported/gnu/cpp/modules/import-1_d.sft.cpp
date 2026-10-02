//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }
export module Foop;
// { dg-module-cmi "Foop" }

import Bar;

export int Thing ();
