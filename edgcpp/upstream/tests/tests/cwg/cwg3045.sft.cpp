//type:fn
//options:--c++26
//options_all:-A -tused

void f()
{
  template for (auto x : { }) {
    int x = 42;                 // error
  }
}

//cwg: 3045
//title: Regularizing environment interactions of expansion statement
//meeting: Kona 11/25
//edg_status: EDGcpfe/28539
