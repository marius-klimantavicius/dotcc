//type: fp
//options: --c++11
// PR c++/71946
// { dg-do compile { target c++11 } }

auto test = []{ __asm__ __volatile__ ("" : : "r" (0) ); };
