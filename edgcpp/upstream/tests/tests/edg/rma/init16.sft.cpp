//options_all:-r -x -tused
//options: --strict;cn

struct A { };
A a, 
  a1 = { },                   // Initializer list for empty class
  a2 = { 0 },                 // Initializer list for empty class
  a3 = a;
struct B { int i; };
B b,
  b1 = { },                   // Empty initializer list
  b2 = { 0 },
  b3 = { 0, 0 },              // Too many initializers
  b4 = b;
struct C { struct A a; };
C c,
  c1 = { },                   // Empty initializer list
  c2 = { 0 },                 // Error?
  c3 = { 0, 0 },              // Too many initializers
  c4 = c;
struct D { int i; struct A a; int j; };
D d,
  d1 = { },                   // Empty initializer list
  d2 = { 0 },
  d3 = { 0, 0 },              // Error?
  d4 = { 0, 0, 0 },           // Error?
  d5 = { 0, 0, 0, 0 },        // Too many initializers
  d6 = d;

