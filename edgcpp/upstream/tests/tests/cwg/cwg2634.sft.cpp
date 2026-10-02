//type:fn
//options_all:--c++23 
auto f(struct X* ptr) {
  struct D {
    private:
      int d;
      friend class X;      // #1
  };
  return D{};
}
X* b = 0;
struct X {
  void show() {
    auto t = f(0);
    t.d = 10;              // #2 error: ::X is not a friend of f::D
  }
};

//cwg: 2634
//title: Avoid circularity in specification of scope for friend class declarations
//meeting: Tokyo 3/24
//edg_status: Passes
