//type:fn
//options_all:--c++26
struct S {
  int y[] = { 0 };          // error: non-static data member of incomplete type
};
