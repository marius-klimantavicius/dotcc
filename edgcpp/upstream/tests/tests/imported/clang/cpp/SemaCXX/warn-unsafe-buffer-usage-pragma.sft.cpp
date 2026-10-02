//type: fp
//options:  --c++20
# 1 "SemaCXX/warn-unsafe-buffer-usage-pragma.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 493 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/warn-unsafe-buffer-usage-pragma.cpp" 2




void basic(int * x) {
  int *p1 = new int[10];
  int *p2 = new int[10];


  p1[5];


# 1 "SemaCXX/warn-unsafe-buffer-usage-pragma.h" 1


p1++;
# 14 "SemaCXX/warn-unsafe-buffer-usage-pragma.cpp" 2

  int *p3 = new int[10];


  p2[5];
  p3[5];
  x++;

# 1 "SemaCXX/warn-unsafe-buffer-usage-pragma.h" 1








p2++;
# 23 "SemaCXX/warn-unsafe-buffer-usage-pragma.cpp" 2
}


void withDiagnosticWarning() {
  int *p1 = new int[10];
  int *p2 = new int[10];



  p1[5];
  p2[5];
#pragma clang diagnostic push
#pragma clang diagnostic warning "-Wunsafe-buffer-usage"
  p1[5];
  p2[5];
#pragma clang diagnostic warning "-Weverything"
  p1[5];
  p2[5];
#pragma clang diagnostic pop



#pragma clang diagnostic push
#pragma clang diagnostic warning "-Wunsafe-buffer-usage"

  p1[5];
  p2[5];

#pragma clang diagnostic pop

  p2[5];
}


void withDiagnosticIgnore() {
  int *p1 = new int[10];
  int *p2 = new int[10];
  int *p3 = new int[10];


  p1[5];
  p2[5];
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wunsafe-buffer-usage"
  p1[5];
  p2[5];
#pragma clang diagnostic ignored "-Weverything"
  p1[5];
  p2[5];
#pragma clang diagnostic pop


#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wunsafe-buffer-usage"

  p1[5];
  p2[5];

#pragma clang diagnostic pop

  p2[5];

#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wunsafe-buffer-usage"

  p1[5];
  p2[5];

  p3[5];
#pragma clang diagnostic pop
}

void noteGoesWithVarDeclWarning() {
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wunsafe-buffer-usage"
  int *p = new int[10];
#pragma clang diagnostic pop

  p[5];
}
