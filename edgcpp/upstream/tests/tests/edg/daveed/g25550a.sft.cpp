//remark:Dependent aggr init
//options:--c++20;fp

  struct B { int x, y; };
  struct D: B {};
  int r = [](auto p) {
            D d = { p.x, p.y };  // Previously an error.
            return p.x+p.y;
          }(D{ 1, 2 });

