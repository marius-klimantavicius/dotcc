// EH identity uses source types, even when structural lowering makes their
// storage/calling signatures alike. Header-free native/C oracle fixture.
struct Big {long long values[6];};
struct A {int n;int call(int x){return n+x;}int safe(int x) noexcept{return n+x+10;}virtual int virt(int x){return n+x+20;}
  Big large(int x){Big b={{n+x,0,0,0,0,0}};return b;}
  void output(Big* p,int x){p->values[0]=n+x;}
};
struct B {int n;int call(int x){return n+x+100;}};
struct D : A {int virt(int x) override{return n+x+30;}};
Big return_big(int n){Big b={{n,0,0,0,0,0}};return b;}
void output_big(Big* p,int n){p->values[0]=n;}
int regular(int n){return n+1;}
int safe(int n) noexcept{return n+2;}
typedef int (A::*MF)(int);
typedef int (A::*NF)(int) noexcept;
typedef int (*FP)(int);
typedef int (*NP)(int) noexcept;
typedef int A::*MD;
typedef const int A::*CD;
int arrays[3]={10,20,30};
int matrix[2][3]={{1,2,3},{4,5,6}};
int run_extended_pointer_exception(int test,void (*hook)()) {
  A a;a.n=7;D d;d.n=9;
  if(test==0){try{throw &A::n;}catch(MD p){hook();return a.*p;}}
  if(test==1){try{throw &A::call;}catch(MF p){hook();return (a.*p)(3);}}
  if(test==2){try{throw &A::virt;}catch(MF p){return (d.*p)(4);}}
  if(test==3){try{throw &A::n;}catch(int B::*){return -1;}catch(MD p){return a.*p;}}
  if(test==4){try{throw &A::n;}catch(long A::*){return -1;}catch(MD p){return a.*p;}}
  if(test==5){try{throw &A::call;}catch(void(A::*)(int)){return -1;}catch(MF p){return (a.*p)(5);}}
  if(test==6){try{throw nullptr;}catch(MD p){return p==nullptr;}}
  if(test==7){try{throw nullptr;}catch(MF p){return p==nullptr;}}
  if(test==8){try{throw &safe;}catch(FP p){hook();return p(6);}}
  if(test==9){try{throw &regular;}catch(NP){return -1;}catch(FP p){return p(7);}}
  if(test==10){try{throw &A::safe;}catch(MF p){return (a.*p)(8);}}
  if(test==11){try{throw &A::call;}catch(NF){return -1;}catch(MF p){return (a.*p)(9);}}
  if(test==12){try{throw &A::n;}catch(CD p){return a.*p;}catch(...){return -2;}}
  if(test==13){try{CD p=&A::n;throw p;}catch(MD){return -1;}catch(CD p){return a.*p;}}
  if(test==14){try{MF p=nullptr;throw p;}catch(MF p){return p==nullptr;}}
  if(test==15){try{MD p=nullptr;throw p;}catch(MD p){return p==nullptr;}}
  if(test==16){try{try{throw &A::n;}catch(MD& p){p=nullptr;throw;}}catch(MD p){return p==nullptr;}}
  if(test==17){try{try{throw &A::call;}catch(MF& p){p=&A::safe;throw;}}catch(MF p){return (a.*p)(10);}}
  if(test==18){try{try{throw nullptr;}catch(MD& p){p=&A::n;hook();throw;}}catch(decltype(nullptr)){return 18;}}
  if(test==19){try{try{throw nullptr;}catch(MF& p){p=&A::call;hook();throw;}}catch(decltype(nullptr)){return 19;}}
  if(test==20){try{throw &regular;}catch(FP p){hook();return p(20);}}
  if(test==21){try{throw nullptr;}catch(FP p){return p==nullptr;}}
  if(test==22){try{throw &regular;}catch(int(*)(long)){return -1;}catch(FP p){return p(22);}}
  if(test==23){try{throw &return_big;}catch(void(*)(Big*,int)){return -1;}catch(Big(*p)(int)){return (int)p(23).values[0];}}
  if(test==24){try{throw &output_big;}catch(Big(*)(int)){return -1;}catch(void(*p)(Big*,int)){Big b;p(&b,24);return (int)b.values[0];}}
  if(test==25){try{throw &arrays;}catch(int(*p)[3]){hook();return (*p)[1];}}
  if(test==26){try{throw &arrays;}catch(int(*)[2]){return -1;}catch(int(*p)[3]){return (*p)[2];}}
  if(test==27){try{throw &arrays;}catch(const int(*p)[3]){return (*p)[0];}}
  if(test==28){try{throw &matrix;}catch(int(*p)[2][3]){return (*p)[1][2];}}
  if(test==29){try{throw nullptr;}catch(int(*p)[3]){return p==nullptr;}}
  if(test==30){try{throw &safe;}catch(NP p){return p(30);}}
  if(test==31){try{throw &A::safe;}catch(NF p){return (a.*p)(31);}}
  if(test==32){try{throw 1.5f;}catch(float n){return n==1.5f;}}
  if(test==33){try{throw 2.5;}catch(float){return -1;}catch(double n){return n==2.5;}}
  if(test==34){try{throw (long double)3.5;}catch(double){return -1;}catch(long double n){return n==(long double)3.5;}}
  if(test==35){try{throw &A::large;}catch(void(A::*)(Big*,int)){return -1;}catch(Big(A::*p)(int)){return (int)(a.*p)(35).values[0];}}
  if(test==36){try{throw &A::output;}catch(Big(A::*)(int)){return -1;}catch(void(A::*p)(Big*,int)){Big b;(a.*p)(&b,36);return (int)b.values[0];}}
  if(test==37){try{throw &A::call;}catch(int(A::*)(int)const){return -1;}catch(MF p){return (a.*p)(37);}}
  if(test==38){try{throw nullptr;}catch(MD* p){return p==nullptr;}}
  if(test==39){try{throw &A::n;}catch(int D::*){return -1;}catch(MD p){return a.*p;}}
  return -99;
}
