//type: fp
//options:  --c++20
# 1 "SemaCXX/warn-unsafe-buffer-usage-fixits-parm-span.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 493 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/warn-unsafe-buffer-usage-fixits-parm-span.cpp" 2




# 1 "SemaCXX/warn-unsafe-buffer-usage-fixits-parm-span.h" 1
void simple(int *p);
# 6 "SemaCXX/warn-unsafe-buffer-usage-fixits-parm-span.cpp" 2

void simple(int *);



void simple(int *p) {

  int tmp;
  tmp = p[5];
}






void twoParms(int *p, int * q) {


  int tmp;
  tmp = p[5] + q[5];
}


void ptrToConst(const int * x) {

  int tmp = x[5];
}
# 44 "SemaCXX/warn-unsafe-buffer-usage-fixits-parm-span.cpp"
void _macro_defined_name_(int * x) {

  int tmp = x[5];
}




namespace {
  void simpleSpecifier(unsigned long long int *p) {

    auto tmp = p[5];
  }


  void attrParm([[maybe_unused]] int * p) {

    int tmp = p[5];
  }


  using T = unsigned long long int;

  void usingTypenameSpecifier(T * p) {

    int tmp = p[5];
  }


  typedef unsigned long long int T2;

  void typedefSpecifier(T2 * p) {

    int tmp = p[5];
  }


  class SomeClass {
  } C;

  void classTypeSpecifier(const class SomeClass * p) {

    if (++p) {}
  }


  struct {

  } ANON_S;

  struct MyStruct {

  } NAMED_S;






  void decltypeSpecifierAnon(decltype(C) * p, decltype(ANON_S) * q, decltype(NAMED_S) * r,
                             decltype(NAMED_S) ** rr) {




    if (++p) {}
    if (++q) {}
    if (++r) {}
    if (++rr) {}
  }


  void decltypeSpecifier(decltype(C) * p, decltype(NAMED_S) * r, decltype(NAMED_S) ** rr) {



    if (++p) {}
    if (++r) {}
    if (++rr) {}
  }




  void macroType(unsigned long int * p, unsigned long long * q) {


    int tmp = p[5];
    tmp = q[5];
  }

}


void decayedArray(int a[]) {

  int tmp;
  tmp = a[5];
}


void decayedArrayOfArray(int a[10][10]) {

  if (++a){}
}

void complexDeclarator(int * (*a[10])[10]) {

  if (++a){}
}



void const_ptr(int * const x) {

  int tmp = x[5];
}


void const_ptr_to_const(const int * const x) {

  int tmp = x[5];
}


void const_volatile_ptr(int * const volatile x) {

  int tmp = x[5];
}


void volatile_const_ptr(int * volatile const x) {

  int tmp = x[5];
}


void const_volatile_ptr_to_const_volatile(const volatile int * const volatile x) {

  int tmp = x[5];
}





static void static_f(int *p) {
  p[5] = 5;
}


static inline void static_inline_f(int *p) {
  p[5] = 5;
}


inline void static static_inline_f2(int *p) {
  p[5] = 5;
}





typedef struct {int x;} UNNAMED_STRUCT;
struct {int x;} VarOfUnnamedType;

void useUnnamedType(UNNAMED_STRUCT * p) {


  if (++p) {

  }
}


void useUnnamedType2(decltype(VarOfUnnamedType) * p) {


  if (++p) {

  }
}
