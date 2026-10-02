//options_all:--c++26
struct S1 { int a, b; };
struct S2 { S1 s, t; };

S2 x[2] = { 1, 2, 3, 4, 5, 6, 7, 8 };
S2 y[2] = {
  {
    { 1, 2 },
    { 3, 4 }
  },
  {
    { 5, 6 },
    { 7, 8 }
  }
};
