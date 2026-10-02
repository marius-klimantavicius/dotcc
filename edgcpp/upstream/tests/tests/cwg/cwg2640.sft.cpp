//type:fn
//options_all:--c++23 
#define z(x) 0
#define a z(
  int x = a\N{abc});

//cwg: 2640
//title: Allow more characters in an n-char sequence
//meeting: Kona 11/22
//edg_status: EDGcpfe/25845
//fixed_in: 6.7
