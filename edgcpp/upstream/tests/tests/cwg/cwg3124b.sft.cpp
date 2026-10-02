//type:fn
//options: -A --c++26

class C {
  friend void g() [[=1]];  // error
};
