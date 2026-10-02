//type:fn
//options: -A --c++20

module;

#define DOT_BAR .bar

export module foo DOT_BAR;  // error

//cwg: 2947
//title: Limiting macro expansion in pp-module
//meeting: Croydon 3/26
