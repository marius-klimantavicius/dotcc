//type: s
//options: --c++11
// PR c++/65801
// { dg-do compile { target c++11 } }
// { dg-options "-Wno-narrowing" }

static struct zai { unsigned int x; } x = {-1};
