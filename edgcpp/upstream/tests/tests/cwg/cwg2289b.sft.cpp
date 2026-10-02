//type: fn
//options_all: -A --c++20 -tused -e 200 --no_wrap
  struct B {} b; template<int> int &get(const B&); 
  auto [B] = b; // ok? 
