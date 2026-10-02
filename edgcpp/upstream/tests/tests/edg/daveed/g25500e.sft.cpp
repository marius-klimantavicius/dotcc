//remark:P2579 -- tweaks to captures
//options:--c++20;fn

  auto lm = [x = 1](int x) { return x; };  // Now an error.
