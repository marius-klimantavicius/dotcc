//type:cp
//options:--c++17;fp:--c++20:--c++20 -DNEG;fn

struct base {
  base();
  base(base const &) = delete;
  base(base &&);
};

struct derived : base {};

void f(bool c) {
  base x;
  try {
    base y;
    try {}
    catch(...) {
#if NEG
      if(c)
        throw x; // Does not move
#endif
      throw y;   // Moves
    }
  } catch(...) {
  }
}

base g(base b, derived d, bool c) {
  if (c)
    throw b;
  return d;
}

#if NEG
base h(base &b, derived &d, bool c) {
  if (c)
    throw b; // Does not move
  return d;  // Does not move
}
#endif

base i(base &&b, derived &&d, bool c) {
  if (c)
    throw b; // Moves in C++20
  return d;  // Moves in C++20
}
