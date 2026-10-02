//type:rp
//options::-DNEG;fn
//options_all:--c++20 --diag_suppress 177

extern "C" int printf(const char *, ...);

void f(int(&)[]) { printf("Called f\n"); }
void g(int(*)[]) { printf("Called g\n"); }

void h(int    (&&)[] )    // #1
  { printf("Called h #1\n"); }
void h(double (&&)[] )    // #2
  { printf("Called h #2\n"); }
void h(int    (&&)[2])    // #3
  { printf("Called h #3\n"); }
void h(int*)              // #4
  { printf("Called h #4\n"); }

int main(int (&arr2)[]) {
  int arr[1];
  int (&r)[] = arr;
  int (*p)[] = &arr;
#if NEG
  int (&r2)[2] = arr2;  // Not allowed
  int (*p2)[2] = &arr2; // Not allowed
#endif
  const int (&r3)[] = {1, 2, 3};
  f(arr);
  g(&arr);

  int *ip = arr;
  h( {1} );          // Calls #1
  h( {1.0} );        // Calls #2
  h( {1.0, 2.0} );   // Calls #2
  h( {1, 2} );       // Calls #3
  h( ip );           // Calls #4
}
