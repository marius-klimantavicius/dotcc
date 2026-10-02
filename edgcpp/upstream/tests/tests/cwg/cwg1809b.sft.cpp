//type:fp
//options_all:--c++17 -tused -A
  template <int> int f(int);
  template <signed char> int f(int);
  int i1 = f<1000>(0);  
