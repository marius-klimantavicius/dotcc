//type:fn
//options_all:--c++20 -tused -A
void f(char*);             // #1
void f(char[]) {}
void f(char *const) {}     // error: redefines #1


