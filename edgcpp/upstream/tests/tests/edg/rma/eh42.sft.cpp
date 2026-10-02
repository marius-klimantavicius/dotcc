//options_all:-r -x -tused
//options: --strict;cn

struct B {
        void f f(double); 
};

struct C : B {};

typedef void (C::*pmfC)(double);

template <class T, pmfC c> struct A {};

template <class T> struct A<T, &B::f> {};

