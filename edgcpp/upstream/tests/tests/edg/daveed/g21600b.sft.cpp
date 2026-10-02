//remark:Composite mem-fun-ptr types
//options:--c++14;fp

  struct S {
    int f();
    int g() const;
  };
  auto pmf = 0 ? &S::f : &S::g;  // Previously accepted.  Now an error.
