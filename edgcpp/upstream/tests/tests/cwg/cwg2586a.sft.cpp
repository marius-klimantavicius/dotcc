//options_all:--c++23 -tused -A
  struct C {
    C& operator=(this C&, C const&);
  };

//cwg: 2586
//title: Explicit object parameter for assignment and comparison
//meeting: Virtual 7/22
//edg_status: EDGcpfe/25524
//fixed_in: 6.10
