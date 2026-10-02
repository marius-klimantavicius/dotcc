//options_all:--c++20 -tused -A
  #include <compare>
  void f(unsigned char i, unsigned ui) {
    i <=> ui;
  }

//cwg: 2528
//title: Three-way comparison and the usual arithmetic conversions
//meeting: Issaquah 2/23
//edg_status: Passes
