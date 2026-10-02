//type:fn
//options_all:--c++23 
  struct S;
  bool operator==(S, S) = default;  // error: S is not complete
  struct S {
    friend bool operator==(S, const S&) = default; // error: parameters of different types
  };
  enum E { };
  bool operator==(E, E) = default;  // error: not a member or friend of a class

//cwg: 2547
//title: Defaulted comparison operator function for non-classes
//meeting: Tokyo 3/24
//edg_status: Passes
