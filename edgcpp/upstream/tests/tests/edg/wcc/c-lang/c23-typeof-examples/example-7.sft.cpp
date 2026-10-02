//type:fp
//options:--c23

// These are not part of the example, but they are required as we don't seem
// to implicitly provide true or NULL in C mode.
#define true 1
#define NULL (void*)0

void f(int);

typeof(f(5)) g(double x) {         // g has type "void(double)"
  printf("value %g\n", x);
}

typeof(g)* h;                      // h has type "void(*)(double)"
typeof(true ? g : NULL) k;         // k has type "void(*)(double)"

void j(double A[5], typeof(A)* B); // j has type "void(double*, double**)"

extern typeof(double[]) D;         // D has an incomplete type
typeof(D) C = { 0.7, 99 };         // C has type "double[2]"

typeof(D) D = { 5, 8.9, 0.1, 99 }; // D is now completed to "double[4]"
typeof(D) E;                       // E has type "double[4]" from D’s completed type
