//remark:Dependent calls
//options:--c++17 -A --exceptions -tused;fp:--c++17;fp

  auto f = [](auto &&f) {
             return static_cast<decltype(f)>(f)();  // Previously an error.
           };                                       // Now okay.


