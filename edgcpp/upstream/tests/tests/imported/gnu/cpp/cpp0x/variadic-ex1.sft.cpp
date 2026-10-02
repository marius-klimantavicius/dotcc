//type: fn
//options: --c++11
// { dg-do compile { target c++11 } }
template<typename ... Elements> class Tuple;
Tuple<>* t; // OK: Elements is empty
Tuple* u; // { dg-error "" }
