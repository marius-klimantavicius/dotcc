//type:fn
//option_all:--c++23
struct X{
  template<class U> X(U){} // #2
};
int main(){
   X x;  // #1
}

//cwg: 2871
//title: User-declared constructor templates inhibiting default constructors
//meeting: St Louis 6/24
//edg_status: Passes
