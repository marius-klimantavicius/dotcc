//type:fp
//options_all:--c++20 -tused -A
void f(char*);             // #1
void f(char[]) {}          // defines #1
void f(const char*) {}     // OK: another overload

void g(char(*)[2]);        // #2
void g(char[3][2]) {}      // defines #2
void g(char[3][3]) {}      // OK: another overload

void h(int x(const int));  // #3
void h(int (*)(int)) {}    // defines #3

