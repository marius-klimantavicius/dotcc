//type:cp
//options::-DNEG;fn:--microsoft:--microsoft -DNEG
//options_all:--c++11 -w

enum class E
{
  A = 0x0, B = 0x1, C = 0x2, D = 0x3
};

struct X {
  E member1 : 2;
  E member2 : 2;
};

X dummy1{E::A, E::B};
X dummy2{{E::A}, {E::B}};
#if NEG
X dummy3{{{E::A}}, {{E::B}}};

X dummy4{0, 1};
X dummy5{{0}, {1}};
X dummy6{{{0}}, {{1}}};
#endif
