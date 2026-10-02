//remark:Variable template partial specialization with requires-clause
//options:--c++20;fp

  template<typename> int v;
  template<typename T> int v<T*>;                // (1)
  template<typename T> requires true int v<T*>;  // Previously an error.

