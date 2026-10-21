#ifndef M1_INTEGRATED_H
#define M1_INTEGRATED_H
#define BODY(x) return x
#define TYPE int
#define ARG 7
template<class T> constexpr T id(T value){BODY(value);}
#endif
