//type:fn
//options_all:--c++20
struct X {
  X();
  explicit X(const X &);
};
X x;
X y = false ? X{} : x; // error: explicit copy constructor
