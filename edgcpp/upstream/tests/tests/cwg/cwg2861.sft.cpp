//type:rp
//remark: Runtime abort since behavior is undefined
//options_all:--c++23
extern "C" int printf(const char* format, ...);

struct A{
    virtual void foo(){}
};
struct B{
    virtual void foo2(){}
};
struct D:A,B{
   virtual void foo(){

   }
};
struct No{};
int main(){
  No no;
  B* ptr = (B*)&no;
  try{
  auto r = dynamic_cast<D*>(ptr);
  printf("%d\n", (long long int) r);
  }catch(...){

  };
}

//cwg: 2861
//title: dynamic_cast on bad pointer value
//meeting: St Louis 6/24
//edg_status: Passes
