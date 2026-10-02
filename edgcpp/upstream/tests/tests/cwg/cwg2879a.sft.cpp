//options_all:--c++23 -A
  typedef int *A[3];                // array of 3 pointer to int
  typedef const int *const CA[3];   // array of 3 const pointer to const int

  auto &&r2 = const_cast<A&&>(CA{}); 
  // OK, temporary materialization conversion is performed

//cwg: 2879
//title: Undesired outcomes with const_cast
//meeting: Wroclaw 11/24
//edg_status: EDGcpfe/27750
