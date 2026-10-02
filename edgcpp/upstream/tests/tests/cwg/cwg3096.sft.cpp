//type:fp
//options:--c++26
//options_all:-A -tused

struct C
{
  int i;
  int j;
};

template<typename = void>
void f()
{
  auto [ ... x ] = C{};
  static_assert(sizeof ... (x) == 0);
}

//cwg: 3096
//title: Value-dependence of size of structured binding pack with non-dependent initializer
//meeting: Kona 11/25
//edg_status: EDGcpfe/28548
//fixed_in: 6.9
