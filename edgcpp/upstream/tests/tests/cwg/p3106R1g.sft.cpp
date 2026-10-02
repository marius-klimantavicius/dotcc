//options_all:--c++26 -tused -A
struct S { } s;
struct A {
  S s1;
  int i1;
  S s2;
  int i2;
  S s3;
  int i3;
} a = {
  { },              // Required initialization
  0,
  s,                // Required initialization
  0
};   
