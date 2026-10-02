//type: fp	
//options_all: -A --c++20 -tused -e 200 --no_wrap -W
//
  int *ptr;
  const int *const &f() {
    return ptr;
  }

//cwg: 2352
//title: Similar types and reference binding
//meeting: Kona 02/19
//edg_status: EDGcpfe/22383
//fixed_in: 6.2
