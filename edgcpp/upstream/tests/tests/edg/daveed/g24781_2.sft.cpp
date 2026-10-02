//remark:Explicit-this member functions
//options:--c++23;fp

struct C {
  void f([[maybe_unused]] this C& self) { }
};
