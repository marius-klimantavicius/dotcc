//type: fp
//options:  --c++20 --c++20 --modules
// { dg-additional-options {-std=c++20 -fmodules-ts} }

export module pr100616_b;
// { dg-module-cmi pr100616_b }

import "100616_a.H";
export C<A{}> c1;
