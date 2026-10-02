//type: fp
//options:  --c++20
# 1 "SemaCXX/warn-unsafe-buffer-usage-fixits-parm-span-overload.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 493 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/warn-unsafe-buffer-usage-fixits-parm-span-overload.cpp" 2






# 1 "SemaCXX/warn-unsafe-buffer-usage-fixits-parm-span-overload.h" 1
void baz();
# 8 "SemaCXX/warn-unsafe-buffer-usage-fixits-parm-span-overload.cpp" 2

void foo(int *p, int * q);

void foo(int *p);

void foo(int *p) {

  int tmp;
  tmp = p[5];
}


void bar(int *p) {

  int tmp;
  tmp = p[5];
}

void bar();


void baz(int *p) {

  int tmp;
  tmp = p[5];
}

namespace NS {


  void foo(int *p) {

    int tmp;
    tmp = p[5];
  }



  void bar(int *p);


}


void NS::bar(int *p) {

  int tmp;
  tmp = p[5];
}


namespace NESTED {
  void alpha(int);
  void beta(int *, int *);

  namespace INNER {


    void alpha(int *p) {

      int tmp;
      tmp = p[5];
    }

  }
}

namespace NESTED {


  void beta(int *p) {

    int tmp;
    tmp = p[5];
  }

  namespace INNER {
    void delta(int);
    void delta(int*);
  }
}



void NESTED::beta(int *p, int *q) {

  int tmp;
  tmp = p[5];
  tmp = q[5];
}

void NESTED::INNER::delta(int * p) {

  int tmp;
  tmp = p[5];
}
