//remark:auto parameters
//options:--c++20;fn

  template<typename T> void g(T) {}
  template<> void g(auto);  // Previously erroneously accepted.

