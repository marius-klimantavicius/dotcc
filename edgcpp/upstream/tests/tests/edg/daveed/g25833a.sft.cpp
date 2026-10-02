//remark:Default operator<=> generation
//options:--c++20;fp

  #include <compare>
  struct S {
    bool operator==(S s) const;
    bool operator<(S s) const;
  };
  template<typename T> struct X {
    S s;
    std::strong_ordering operator<=>(X const&) const = default;
  };
  X<int> x;
  auto r = x <=> x;  // Previously an error.  Now okay.
