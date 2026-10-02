// POD return values whose array subobjects must remain addressable through
// the full expression, including inside generated sequence/EH helpers.
struct Packet {int values[3];};
struct Nested {Packet packet;int tail;};
int materialize_calls,materialize_live,materialize_destroyed;
Packet make_packet(int n){++materialize_calls;Packet p={{n,n+1,n+2}};return p;}
Nested make_nested(int n){++materialize_calls;Nested p={{{n,n+1,n+2}},n+3};return p;}
Packet fail_packet(){throw 77;}
int inspect_packet(const int* p,void(*hook)()) {
  volatile int occupied[256]={};for(int i=0;i<256;++i)occupied[i]=i;
  hook();return p[0]+p[1]+p[2]+occupied[255]-255;
}
int inspect_two(const int* a,const int* b,void(*hook)()) {
  hook();return a!=b && a[0]==3 && a[2]==5 && b[0]==4 && b[2]==6;
}
struct MaterializeGuard {
  MaterializeGuard(){++materialize_live;}
  ~MaterializeGuard(){--materialize_live;++materialize_destroyed;}
};
void materialize_throw_hook(){throw 19;}
int run_materialize(int test,void(*hook)()) {
  materialize_calls=materialize_live=materialize_destroyed=0;
  if(test==0)return make_packet(1).values[2];
  if(test==1)return make_nested(2).packet.values[1];
  if(test==2)return inspect_packet(make_packet(3).values,hook);
  if(test==3)return inspect_two(make_packet(3).values,make_packet(4).values,hook);
  if(test==4)return inspect_packet((materialize_calls=10,make_packet(4)).values,hook);
  if(test==5)return inspect_packet((materialize_calls==0?make_packet(5):fail_packet()).values,hook);
  if(test==6)return inspect_packet((materialize_calls!=0?fail_packet():make_packet(6)).values,hook);
  if(test==7){int sum=0;for(int i=0;i<20;++i)sum+=make_packet(i).values[2];return sum;}
  if(test==8){int i=0,sum=0;while(make_packet(i++).values[0]<4)sum+=i;return sum;}
  if(test==9){Packet (*f)(int)=make_packet;return inspect_packet(f(9).values,hook);}
  if(test==10){try{throw make_packet(10).values[1];}catch(int n){hook();return n;}}
  if(test==11){try{MaterializeGuard guard;return inspect_packet(make_packet(11).values,materialize_throw_hook);}catch(int n){hook();return n;}}
  if(test==12){try{return inspect_packet((materialize_calls==0?fail_packet():make_packet(12)).values,hook);}catch(int n){return n;}}
  if(test==13){return make_nested(13).packet.values[(materialize_calls+=10,1)];}
  if(test==14){MaterializeGuard guard;return inspect_packet((make_packet(0),make_packet(14)).values,hook);}
  if(test==15){int sum=0;for(int i=0;i<3;++i){try{MaterializeGuard guard;throw make_nested(i).packet.values[2];}catch(int n){sum+=n;hook();}}return sum;}
  return -1;
}
int read_materialize_calls(){return materialize_calls;}
int read_materialize_live(){return materialize_live;}
int read_materialize_destroyed(){return materialize_destroyed;}
