//type: fp
//options: --c++11
// PR c++/59938
// { dg-do compile { target c++11 } }

struct Data { const char* const& name; };
constexpr Data d = { "" };
