//type:fn
//options_all:--c++17 -tused -A
  template<typename T> struct X; 
  struct X<int> { 
  }; 

//cwg: 2234
//title: Missing rules for simple-template-id as class-name
//meeting: Jacksonville 2/18
//edg_status: Passes
