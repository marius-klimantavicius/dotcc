//type: fp
//options: --c++11
// PR c++/91678 - wrong error with decltype and location wrapper.
// { dg-do compile { target c++11 } }

float* test(float* c) { return (decltype(c + 0))(float*)c; }
