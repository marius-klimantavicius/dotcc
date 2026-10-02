//type: fp
//options: --c++11
// { dg-do compile { target c++11 } }

enum class E { };
E f();
bool b2 = static_cast<bool>(f());
