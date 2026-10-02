//type:fp
//options_all:--c++14 -tused -A
  int n;
  void f() {
   constexpr int &r = n;
   [] { return r; }; 
  }
