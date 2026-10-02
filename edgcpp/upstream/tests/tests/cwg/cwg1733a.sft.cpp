//type:fn
//options_all:--c++20 -tused -A
  struct B {
   	B&& operator=(B const&) && = default;
  	};

//cwg: 1733
//title: Return type and value for operator= with ref-qualifier
//meeting: Virtual 10/21
//edg_status: Passes
