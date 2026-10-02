//type: fp
//options: 
# 1 "SemaCXX/warn-static-function-inheader.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 482 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/warn-static-function-inheader.cpp" 2
# 1 "SemaCXX/warn-static-function-inheader.h" 1
static void thing(void) {
}
# 2 "SemaCXX/warn-static-function-inheader.cpp" 2


static void another(void) {
}

template <typename T>
void foo(void) {
  thing();
  another();
}
