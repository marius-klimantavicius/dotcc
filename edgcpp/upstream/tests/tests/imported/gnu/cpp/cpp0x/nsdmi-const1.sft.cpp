//type: fp
//options: --c++11
// PR c++/50707
// { dg-do compile { target c++11 } }

int g;

struct S {
   int const v=g;
};

S s;
