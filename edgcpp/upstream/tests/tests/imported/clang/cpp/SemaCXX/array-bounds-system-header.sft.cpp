//type: fp
//options: 
# 1 "SemaCXX/array-bounds-system-header.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 482 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/array-bounds-system-header.cpp" 2

# 1 "SemaCXX/Inputs/array-bounds-system-header.h" 1
# 3 "SemaCXX/array-bounds-system-header.cpp" 2
void test_system_header_macro() {
  int i[3]; i[3] = 5;
  char a[3];
  (a)[(3)] = 5;
  sizeof(a) > 3 ? (a)[3] = 5 : 5;
  (a[3] = 5);
}
