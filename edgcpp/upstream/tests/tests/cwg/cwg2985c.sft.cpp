//type:fp
//options:--c++23 -A
struct X {
  X() = default;
  X(X&&) = delete;
};
struct Y : X {};
struct Z {
  operator Y() { return Y(); }
};
X&& x = Z();
