//type:fp
//options_all:--c++17 -tused -A

template <typename T>  struct A {
   static void foo();
};

A<void> a; // refer to specialization before default arguments added

template <typename T = short> struct A;

template <> void A<void>::foo() { // use same specialization
   A< > *ap;} // needs default argument to be transferred to the injected class name

//cwg: 1922
//title: Injected class template names and default arguments
//meeting: Lenexa 5/15
//edg_status: Passes
