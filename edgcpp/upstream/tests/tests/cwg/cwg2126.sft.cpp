//type:fp
//options_all:--c++17 -tused -A
//

  typedef const int CI[3];
  constexpr CI &ci = CI{11, 22, 33};
  static_assert(ci[1] == 22, "");

//cwg: 2126
//title: Lifetime-extended temporaries in constant expressions
//meeting: Belfast 11/19
//edg_status: EDGcpfe/22905
//fixed_in: 6.2
