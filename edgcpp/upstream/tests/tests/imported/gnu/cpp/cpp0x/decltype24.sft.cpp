//type: fp
//options: --c++11
// PR c++/47068
// { dg-do compile { target c++11 } }

template <class T> struct broken {
  int member;
  typedef decltype(~ member) gcc_crashes_here;
};
