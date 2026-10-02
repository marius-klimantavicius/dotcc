// Data/member-function pointer semantics. All pointer operations are defined;
// the EDG C backend is the value/side-effect oracle.
unsigned long long trace;
void (*observer)();
void mark(int n){trace=trace*131+(unsigned)n;observer();}
struct Plain {int x,y;};
union Choice {int x,y;};
struct Left {
  int a;
  Left():a(11){}
  int direct(int n){mark(4);return a+n;}
  virtual int first(int n){mark(5);return a+n+100;}
  virtual int second(int n){mark(6);return a+n+200;}
  int qualified(int n) const noexcept {mark(7);return a+n+300;}
};
struct Right {
  long long pad;
  int b;
  Right():pad(17),b(22){}
  int direct(int n){mark(8);return b+n;}
  virtual int right(int n){mark(9);return b+n+400;}
};
struct Both : Left,Right {
  int c;
  Both():c(33){}
  int first(int n) override {mark(10);return a+b+c+n;}
  int right(int n) override {mark(11);return b+c+n;}
  int own(int n){mark(12);return c+n;}
  int ref(int n) & {return c+n+500;}
  int ref(int n) && {return c+n+600;}
};
struct VLeft : virtual Left {int v;VLeft():v(7){} int first(int n)override{return a+v+n;}};
struct VRight : virtual Left {int w;VRight():w(9){}};
struct Diamond : VLeft,VRight {int first(int n)override{mark(13);return a+v+w+n;}};
typedef int (Left::*LF)(int);
typedef int (Both::*BF)(int);
typedef int (Right::*RF)(int);
typedef int Both::*BD;
struct Holder {BF method;BD data;};
BF return_method(int n){return n?static_cast<BF>(&Right::right):&Both::own;}
BD return_data(int n){return n?static_cast<BD>(&Right::b):&Both::c;}
int invoke(Both* p,BF f,int n){return (p->*f)(n);}
BF choose_method(){mark(2);return &Both::own;}
Both* choose_object(Both* p){mark(1);return p;}
BD choose_data(){mark(2);return &Both::c;}
int argument(){mark(3);return 5;}
unsigned long long read_member_trace(){return trace;}
int run_member_case(int test,void (*hook)()) {
  trace=0;observer=hook;
  Plain plain={3,7};Both object;Diamond diamond;
  if(test==0){int Plain::*p=&Plain::x;return plain.*p;}
  if(test==1){int Plain::*p=&Plain::y;Plain* q=&plain;return q->*p;}
  if(test==2){int Plain::*p=0;return p==nullptr && p!=&Plain::x;}
  if(test==3){int Plain::*p=&Plain::x;return p!=nullptr && p==&Plain::x && p!=&Plain::y;}
  if(test==4){int Plain::*p=&Plain::x;plain.*p=14;return plain.x;}
  if(test==5){const int Plain::*p=&Plain::y;const Plain* q=&plain;return q->*p;}
  if(test==6){Choice c;int Choice::*p=&Choice::y;c.*p=19;return c.y;}
  if(test==7){BD p=&Right::b;return object.*p;}
  if(test==8){BD p=&Right::b;int Right::*q=static_cast<int Right::*>(p);return object.*q;}
  if(test==9){BD p=&Both::c;int Right::*q=static_cast<int Right::*>(p);Right* r=&object;return r->*q;}
  if(test==10){int Right::*p=nullptr;BD q=p;return q==nullptr;}
  if(test==11){BD p=nullptr;int Right::*q=static_cast<int Right::*>(p);return q==nullptr;}
  if(test==12){int Left::*p=&Left::a;return diamond.*p;}
  if(test==13){BD p[3]={&Left::a,&Right::b,&Both::c};return object.*p[0]+object.*p[1]+object.*p[2];}
  if(test==14){BD p=return_data(test&1);return object.*p;}
  if(test==15){choose_object(&object)->*choose_data()=argument();return object.c;}
  if(test==16){LF f=&Left::direct;return (object.*f)(1);}
  if(test==17){LF f=&Left::first;return (object.*f)(2);}
  if(test==18){LF f=&Left::second;return (object.*f)(3);}
  if(test==19){BF f=&Right::direct;return (object.*f)(4);}
  if(test==20){BF f=&Right::right;return (object.*f)(5);}
  if(test==21){BF f=&Both::own;return invoke(&object,f,6);}
  if(test==22){BF f=nullptr;return f==nullptr && f!=&Both::own;}
  if(test==23){BF f=&Both::own;return f!=nullptr && f==&Both::own && f!=static_cast<BF>(&Left::direct);}
  if(test==24){LF f=&Left::first;return f!=nullptr && f==&Left::first && f!=&Left::second;}
  if(test==25){RF f=nullptr;BF q=f;return q==nullptr;}
  if(test==26){BF f=nullptr;RF q=static_cast<RF>(f);return q==nullptr;}
  if(test==27){BF f=&Right::right;RF q=static_cast<RF>(f);return (object.*q)(7);}
  if(test==28){BF f=&Both::own;RF q=static_cast<RF>(f);Right* r=&object;return (r->*q)(8);}
  if(test==29){LF f=&Left::first;return (diamond.*f)(9);}
  if(test==30){LF f=&Left::direct;return (diamond.*f)(10);}
  if(test==31){int (Left::*f)(int)const noexcept=&Left::qualified;return (object.*f)(11);}
  if(test==32){int (Left::*f)(int)const=&Left::qualified;return (object.*f)(12);}
  if(test==33){BF f=return_method(1);return (object.*f)(13);}
  if(test==34){BF f[3]={&Left::direct,&Right::right,&Both::own};int sum=0;for(int i=0;i<3;++i)sum+=(object.*f[i])(i);return sum;}
  if(test==35){Holder h={&Right::right,&Both::c};Holder copy=h;return (object.*copy.method)(object.*copy.data);}
  if(test==36){return (choose_object(&object)->*choose_method())(argument());}
  if(test==37){int (Both::*f)(int) & = &Both::ref;return (object.*f)(14);}
  if(test==38){int (Both::*f)(int) && = &Both::ref;return (static_cast<Both&&>(object).*f)(15);}
  if(test==39){BF f=&Right::right;BF other=f;f=nullptr;return other!=f && other==static_cast<BF>(&Right::right);}
  if(test==40){const BF f=&Both::own;const BF* p=&f;return (object.**p)(16);}
  if(test==41){BF f=&Both::own;BF* p=&f;*p=static_cast<BF>(&Right::right);return (object.**p)(17);}
  if(test==42){BF f=return_method(0);observer();return (object.*f)(18);}
  if(test==43){BD p=return_data(1);observer();return object.*p;}
  if(test==44){LF f=&Left::first;Left base;return (base.*f)(19);}
  if(test==45){RF f=&Right::right;Right base;return (base.*f)(20);}
  if(test==46){LF f=&Left::first;VLeft* p=&diamond;return (p->*f)(21);}
  if(test==47){LF f=&Left::second;VRight* p=&diamond;return (p->*f)(22);}
  return -1;
}
