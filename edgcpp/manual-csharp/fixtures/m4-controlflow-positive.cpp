// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored scalar mutation, alias, conversion, scope and loop witness input.
constexpr int& destination(int& value){value+=10; return value;}
constexpr int source(const int& value){return value;}
constexpr int order(){int value=1; destination(value)=source(value); destination(value)+=source(value); return value;}
constexpr int mutate(int value){int& alias=value; (alias=2)=3; alias+=4; return value;}
constexpr int initialized(){int value; value=4; return value;}
constexpr int branches(int input){int value=0; if(input){int local=3; value=local;} else {int local=8; value=local;} return value;}
constexpr int count(int limit){int i=0; int total=0; while(i<limit){i+=1; if(i==2) continue; {const int& item=i; total+=item;} if(i==4) break;} return total;}
constexpr int counted(){int total=0; for(int i=0;i<6;i+=1){if(i==2) continue; if(i==4) break; total+=i;} return total;}
constexpr int once(){int i=0; do {i+=1; if(i<3) continue; break;} while(i<8); return i;}
static_assert(order()==12);
static_assert(mutate(99)==7);
static_assert(initialized()==4);
static_assert(branches(1)==3);
static_assert(branches(0)==8);
static_assert(count(9)==8);
static_assert(counted()==4);
static_assert(once()==3);
