template<typename T>
struct X {
  struct N;
};

struct Y {
  friend struct X<double>::N;
};
