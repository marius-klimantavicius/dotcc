//options_all:-r -x -tused
//options: --strict;cn:;cn

// multiple access paths

   class A{
   public:
           virtual void f();
   };
    
   class AA : virtual protected A {
   };

   class AAA : virtual public AA {
   };

   class B : virtual private A {};
    
   class C : virtual private A {};
    
   class D : public B, public C, virtual public AAA {};

   void A::f() { }
    
   main(){
           D *d = new D;
           d->f();
   }


