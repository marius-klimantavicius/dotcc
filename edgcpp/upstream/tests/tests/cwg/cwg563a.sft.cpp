//type:fp
//options_all:--c++20 -tused -A 
extern "C"                      // f1 and its function type have C language linkage;
  void f1(void(*pf)(int));      // pf is a pointer to a C function

extern "C" typedef void FUNC();
FUNC f2;                        // f2 has C++ language linkage and 
                                // its type has C language linkage

extern "C" FUNC f3;             // f3 and its type have C language linkage

void (*pf2)(FUNC*);             // the variable pf2 has C++ linkage and its type
                                // is "pointer to C++ function that takes one parameter of type
                                // pointer to C function"
extern "C" {
  static void f4();             // the name of the function f4 has internal linkage, 
                                // so f4 has no language linkage; its type has C language linkage.
}

//cwg: 563
//title: Linkage specification for objects
//meeting: Virtual 11/20*
//edg_status: Passes
