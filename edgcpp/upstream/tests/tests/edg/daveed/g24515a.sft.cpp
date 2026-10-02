//remark:Function address binding
//options:--c++17;fp

  int g(int);
  int g() noexcept;
  int (&rf)() = g;
