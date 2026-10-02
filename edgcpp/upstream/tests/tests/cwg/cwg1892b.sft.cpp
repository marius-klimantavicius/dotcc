//type:fn
//options_all:--c++20 -A
template<typename T> using X = T;
  X<auto()> f_with_deduced_return_type; 
