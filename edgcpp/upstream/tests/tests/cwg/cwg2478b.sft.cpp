//options_all:--c++20 -tused -A
  template<typename T> struct S {
    static constinit T x;
  };
  template<> int S<int>::x = 10;    // constinit required?
  extern char c;
  template<> char S<char>::x = c;  // error, c not constant?
