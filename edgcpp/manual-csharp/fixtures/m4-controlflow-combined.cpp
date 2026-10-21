// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored macro/template/frozen-overload/reference/storage/control-flow gate.
#define STEP(value) value += 1
#define STORE(destination, source) destination = source
constexpr int select(const int& value){return value+1;}
constexpr long select(const long& value){return value+2;}
template<class T> constexpr T accumulate(T limit){
    T total=0;
    T i=0;
    while(i<limit){
        STEP(i);
        if(i==2) continue;
        {const T hold=select(i); total+=hold;}
        if(i==4) break;
    }
    return total;
}
void select(int&);
constexpr int combined(){int value=0; int& alias=value; STORE(alias,accumulate(5)); return value+accumulate(5L);}
static_assert(combined()==25);
static_assert(accumulate(3)==6);
static_assert(accumulate(2L)==3);
