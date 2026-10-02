//options_all:-r -x -tused
//options: --cfront_3.0;cp

// Test for class layout compatibility with cfront
struct A { int a; };
struct B { int b; };
struct C : virtual A { int c; };
struct D : virtual A, virtual B { int d; };
struct F : C, virtual D, virtual A { int f; };

