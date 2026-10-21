#ifndef M1_DIFFICULT_H
#define M1_DIFFICULT_H
#define BODY(x) return x
#define NUMBER 4
constexpr long mark(long x){return x+3;}
template<class T> constexpr T id(T x){T local=x; BODY(local);}
template<class T> constexpr long combine(T x){T copy=id(x); BODY(mark(0)+copy);}
void sink(int);
constexpr int pick(int x){return x;}
template<class T> constexpr T pick(T x){return sink(x);}
#endif
