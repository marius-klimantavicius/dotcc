//type:fn
//options_all:--c++23 -tused
  int* p;
  const int*&& r = static_cast<int*&&>(p);

//cwg: 2801
//title: Reference binding with reference-related types
//meeting: Kona 11/23
//edg_status: EDGcpfe/26812
