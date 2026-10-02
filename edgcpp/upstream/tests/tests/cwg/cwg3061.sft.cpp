//type:fp
//options:--c++26
//options_all:-A -tused

void f()
{
  template for (int x : { 1, })
  { }
}

//cwg: 3061
//title: Trailing comma in an expansion-init-list
//meeting: Kona 11/25
//edg_status: EDGcpfe/28544
