//type:fp
//options_all:--c++17 -w -tused -A

class A_ { ~A_(); };
class B_ { A_ a = {}; };

//cwg: 2227
//title: Destructor access and default member initializers
//meeting: Jacksonville 2/18
//edg_status: EDGcpfe/21913
