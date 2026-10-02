//type:fp
//options:--c++11 --set_flag no_very_expensive_checking --pending_instantiations 1000

template<int N> struct C : C<N-1>
{ };

template<>
struct C<0>
{
  int i;
};

C<500> c500;
