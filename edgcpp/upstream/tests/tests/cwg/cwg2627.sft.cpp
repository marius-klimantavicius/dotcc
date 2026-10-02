//type:fp
//options_all:--c++20 -tused -A
#include <compare>
struct C {
  long long i : 8;
};

void f() {
  C x{1}, y{2};
  x.i <=> y.i; 
}

//cwg: 2627
//title: Bit-fields and narrowing conversions
//meeting: Kona 11/22
//edg_status: EDGcpfe/25828
