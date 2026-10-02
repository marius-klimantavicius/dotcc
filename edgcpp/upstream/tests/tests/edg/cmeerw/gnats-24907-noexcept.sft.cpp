//type:fp
//options:--c++20:--ms_c++20 --microsoft_version 1938
//options_all:-tused -w

template<int x>
struct A
{
  int f ( )
  {
    requires {
      A { noexcept ( A { } ) };
    };
    return 0;
  }
};

int i = A<1>().f();
