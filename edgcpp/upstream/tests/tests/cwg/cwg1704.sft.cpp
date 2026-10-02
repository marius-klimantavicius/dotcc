//type:fn
//options_all:--c++17 -tused -A

template<typename T> T var = {};
   template int *var<int>;      // error: instantiated variable has type int

//cwg: 1704
//title: Type checking in explicit instantiation of variable templates
//meeting: Toronto 7/17
//edg_status: EDGcpfe/21991
