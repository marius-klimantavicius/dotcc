//type: fp
//options: --c++11 --c++03 --no_strict_gnu
// PR c++/50810
// { dg-do compile { target c++11 } }
// { dg-options "-std=gnu++98 -Wc++11-compat" }

signed char data[] = { 0xff }; // { dg-warning "narrowing" }
