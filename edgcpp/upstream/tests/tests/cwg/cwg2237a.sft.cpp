//type:fn 
//options_all:--c++17 -tused -A
template<class T>
  struct A {
    A<T>();  // error: simple-template-id not allowed for constructor
    A(int);  // OK, injected-class-name used
  };

//cwg: 2237
//title: Can a template-id name a constructor?
//meeting: Jacksonville 2/18
//edg_status: EDGcpfe/21958
