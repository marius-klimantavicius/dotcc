//type:fn
//options_all:--c++20 -A
  void f(int n) {
    void g(), h(), i();
    switch (n) {
    case 1:
    case 2:
      g();
      [[fallthrough]];
    case 3:              // warning on fallthrough discouraged
      do {
        [[fallthrough]]; // error: next statement is not part of the same substatement execution
      } while (false);
    case 6:
      do {
        [[fallthrough]]; // error: next statement is not part of the same substatement execution
      } while (n--);
    case 7:
      while (false) {
        [[fallthrough]]; // error: next statement is not part of the same substatement execution
      }
    case 5:
      h();
    case 4:              // implementation may warn on fallthrough
      i();
      [[fallthrough]];   // ill-formed
    }
  }

//cwg: 2406
//title: [[fallthrough]] attribute and iteration statements
//meeting: Cologne 07/19
//edg_status: EDGcpfe/21578
//fixed_in: 6.0
