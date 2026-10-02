//type:cp
//options::-DNEG;fn
//options_all:--c++17

struct X {
#ifndef NEG
  X() = default;
#else
  X() = delete;
#endif
};

struct Y : X {
  using X::X;
  Y(void*); // Suppresses generation of Y::Y()
  int yi = 20;
};

Y y; // Calls X::X() - a trivial ctor
