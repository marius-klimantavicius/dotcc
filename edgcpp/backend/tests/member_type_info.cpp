#include <typeinfo>
struct RttiA {int a;virtual ~RttiA(){};int f(int n){return a+n;}int g(int n)const{return a+n;}};
struct RttiB {int b;virtual ~RttiB(){};};
struct RttiD:RttiA,RttiB {int d;};
using RM=int(RttiA::*)(int);
using RCM=int(RttiA::*)(int)const;
using RN=int(RttiA::*)(int)noexcept;
using RBM=int(RttiB::*)(int);
const std::type_info& member_info(int i) {
  if(i==0)return typeid(int RttiA::*);
  if(i==1)return typeid(const int RttiA::*);
  if(i==2)return typeid(int RttiB::*);
  if(i==3)return typeid(RM);
  if(i==4)return typeid(RCM);
  if(i==5)return typeid(RN);
  if(i==6)return typeid(RBM);
  if(i==7)return typeid(RM*);
  if(i==8)return typeid(RM[2]);
  if(i==9)return typeid(int(*)(int));
  if(i==10)return typeid(int(*)(int)noexcept);
  return typeid(RM const);
}
const char* member_info_name(int i){return member_info(i).name();}
const void* member_info_address(int i){return &member_info(i);}
int run_member_info(void (*hook)()) {
  RttiD d;RttiA* a=&d;RttiB* b=&d;hook();
  if(dynamic_cast<RttiB*>(a)!=b || dynamic_cast<RttiD*>(b)!=&d)return -1;
  for(int i=0;i<12;++i)for(int j=0;j<12;++j){
    bool same=i==j || (i==3 && j==11) || (i==11 && j==3);
    if((member_info(i)==member_info(j))!=same)return -100-i;
  }
  const auto& info=typeid(&RttiA::f);const char* name=info.name();hook();
  if(info!=typeid(RM) || &info!=&typeid(RM) || name!=info.name())return -2;
  try{throw &RttiA::f;}catch(RCM){return -3;}catch(RM m){if(typeid(m)!=info)return -4;}
  try{throw &RttiA::a;}catch(int RttiB::*){return -5;}catch(int RttiA::*m){if(typeid(m)!=member_info(0))return -6;}
  return 1;
}
