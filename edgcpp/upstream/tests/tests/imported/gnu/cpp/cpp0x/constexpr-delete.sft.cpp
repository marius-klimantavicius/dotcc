//type: fp
//options: --c++11
// { dg-do compile { target c++11 } }

constexpr bool never() = delete; // useless, but OK
