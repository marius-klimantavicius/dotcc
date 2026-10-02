//type:fn
//options:--c++23:--c++23 --microsoft

template<int I>
void foo()
{
  if constexpr (true) ;
  else if 1 ;

  if constexpr (true) ;
  else if ! 1 ;

  if constexpr (true) ;
  else if ! (1) ;
}

template void foo<1>();
