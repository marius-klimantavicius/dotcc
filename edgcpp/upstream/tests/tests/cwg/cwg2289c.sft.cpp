//type: fn
//options_all: -A --c++20 -tused -e 200 --no_wrap
  struct C {}; int c[1]; 
  auto [C] = c; // ok?
