//remark:auto(x)/auto{x}
//options:--c++23;fp

  struct S {};
  S& g();
  int f(S&);  // (1)
  int f(S&&); // (2)
  int x = f(g()),        // Calls (1).
      y = f(auto(g()));  // Calls (2), materializing a temporary.

