//remark:reinterpret_cast with no effect
//options:--c++11 -A;fp:--c++11;fp:--c++03;fn

  int p = 1, q = reinterpret_cast<int>(p);  // Now accepted in C++11 mode.

