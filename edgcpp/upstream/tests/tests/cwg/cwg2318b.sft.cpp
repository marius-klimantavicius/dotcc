//type: fp
//options_all: -A --c++20 -tused -e 200 --no_wrap

template<typename T, int N> void o(T (* const (&)[N])(T)) { }
  int f1(int);
  int f4(int);
  char f4(char);
  o({ &f1, &f4 }); // OK, T deduced as int from first element, nothing deduced from second element, N deduced as 2
