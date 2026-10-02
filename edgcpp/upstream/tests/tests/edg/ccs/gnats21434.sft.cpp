//type:fn
//options_all:--c++11

struct X {
  explicit constexpr X(int){};
};

struct Y {
  explicit Y(int){};
};

struct S {
  X x;
  Y y;
};

S ss1 { {3}, {3} };
