//type:fn
//options: -A --c++26 --set_flag reflection

void f(int p)
  pre([&] { ++p; return true; }())         // error
  pre([&] { ++[:^^p:]; return true; }())   // error
{}

//cwg: 3158
//title: Constification for splice-expressions
//meeting: Croydon 3/26
