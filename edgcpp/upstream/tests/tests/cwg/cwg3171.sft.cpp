//type:fn
//options: -A --c++20

module;

int f();

export module M;

int f();  // error

//cwg: 3171
//title: Codify the strong ownership for modules
//meeting: Croydon 3/26
