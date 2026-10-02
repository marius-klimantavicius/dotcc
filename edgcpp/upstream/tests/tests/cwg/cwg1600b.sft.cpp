//type:fp
//options_all:--c++14 -tused -A
  const int&& foo();
  decltype(foo()) x1 = 17; // type is const int&&
