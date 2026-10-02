//type: fp
//options: --c++23
// PR c++/108525
// { dg-do compile { target c++23 } }

auto b = [](...) static { return 1; };
auto foo () { return b (); }
