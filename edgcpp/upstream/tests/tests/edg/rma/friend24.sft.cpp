//options_all:-r -x -tused
//options: --strict;cp

class A {
  int i;
};
static class N {
  private:                      
    static int o;               
} Nt;
 
class B : public N {            
  public:                       
    static B* const n;          
  protected:            
    B(A&);
};
class C : public  B {
  private:                      
    static long r;      
    friend B::B(A&);
};

