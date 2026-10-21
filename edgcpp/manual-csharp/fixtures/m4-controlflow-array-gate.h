// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
#ifndef CONTROLFLOW_ARRAY_GATE_H
#define CONTROLFLOW_ARRAY_GATE_H
#define DATA u"ab" "c"
#define STORE(left,right) left+=right
#define RET(value) return value
constexpr const char16_t& helper(const char16_t& value){RET(value);}
constexpr const int& helper(const int& value){RET(value);}
template<class T> constexpr int collect(T limit){
  char16_t cells[]=DATA;
  int index=0;
  int total=0;
  while(index<limit){
    if(index==1){index+=1;continue;}
    STORE(cells[index],index);
    total+=helper(cells[index]);
    if(index==2)break;
    index+=1;
  }
  RET(total);
}
#endif
