//type:fn
//options_all:--c++23

 constexpr int f() {
    const int &x = 42;
    const_cast<int &>(x) = 1;  // undefined behavior
    return x;
  }
  constexpr int z = f();   // error: not a constant expression

//cwg: 2657
//title: Cv-qualification adjustment when binding reference to temporary
//meeting: Tokyo 3/24
//edg_status: Passes
