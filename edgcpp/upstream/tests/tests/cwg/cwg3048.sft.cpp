//type:fp
//options:--c++26
//options_all:-A -tused

struct S { };
void f() {
  S s;
  template for (auto x : s) { }
}

//cwg: 3048
//title: Empty destructuring expansion statements
//meeting: Kona 11/25
//edg_status: EDGcpfe/28540
