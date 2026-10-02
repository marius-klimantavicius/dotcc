//type:fp
//options_all:-tused -A --c++17
//
  template<typename> struct s {};
  ::template s<void> q; 

//cwg: 1929
//title: template keyword following namespace nested-name-specifier
//meeting: Lenexa 5/15
//edg_status: EDGcpfe/19904
//fixed_in: 6.7
