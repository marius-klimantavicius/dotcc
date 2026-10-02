//type:cp
//options:--c++20:--c++17;fn

void f(int(&)[]);
void g(int(*)[]);
void h(int    (&&)[] );    // #1
void h(double (&&)[] );    // #2
void h(int    (&&)[2]);    // #3
void h(int*);              // #4

void f() {
  int arr[1];
  int *ip = arr;
  int (&r)[] = arr;  // Now accepted
  int (*p)[] = &arr; // Now accepted
  f(arr);            // Now accepted
  g(&arr);           // Now accepted
  h( {1} );          // Calls #1
  h( {1.0} );        // Calls #2
  h( {1.0, 2.0} );   // Calls #2
  h( {1, 2} );       // Calls #3
  h( ip );           // Calls #4
}
