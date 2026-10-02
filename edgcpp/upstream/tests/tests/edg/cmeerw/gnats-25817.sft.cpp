//type:fp
//options:--c++17:--c++17 --no_parse_templates:--c++20:--ms_c++17:--ms_c++20
//options_all:-w

namespace minimal
{
  template<int> struct X { };
  template<bool B>
  bool f() {
    if constexpr (B) {
      int X = 0;
      return X < 0;
    }
    return false;
  }
  template bool f<false>();
}

template<class T> struct CT { };
template<int N> struct CN { };


template<class U>
void less_than_non_type(U)
{
  if constexpr (false) {
    int CN = 5;
    (CN < 10);
    CN < 10;
  }
}

template void less_than_non_type(int);


template<class U>
void less_than_type(U)
{
  if constexpr (false) {
    int CT = 5;
    (CT < int());
    CT < int();
  }
}

template void less_than_type(int);


template<class U>
void non_compound_statement(U)
{
  if constexpr (false)
    int CN = &CN < &CN;
}

template void non_compound_statement(int);
