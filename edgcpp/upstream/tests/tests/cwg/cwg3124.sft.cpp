//type:fn
//options: -A --c++26 --set_flag reflection

void f() {
  extern int x [[=1]];  // error
}

//cwg: 3124
//title: Disallow annotations on block-scope externs and non-unique friend declarations
//meeting: Croydon 3/26
