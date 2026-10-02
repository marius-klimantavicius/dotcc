//type: fp
//options:  --c++20 --modules
// { dg-do compile { target *-*-*gnu* } }
// { dg-additional-options "-fmodules" }

export module M;
export inline int x = 0;
