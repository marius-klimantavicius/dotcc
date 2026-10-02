//type:fn
//options:--c++26
//options_all:-A -tused

template<typename T>
struct C
{ };

namespace ns
{
  template struct C<int>;       // error
}

//cwg: 3066
//title: Declarative nested-name-specifier in explicit instantiation
//meeting: Kona 11/25
//edg_status: Passes
