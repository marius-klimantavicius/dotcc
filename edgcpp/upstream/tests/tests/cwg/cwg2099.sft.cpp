//type:fp
//options_all:--c++17 -tused -A 
//
  struct X {
    static constexpr int arr[] = { 1, 2, 3 };
  };

//cwg: 2099
//title: Inferring the bound of an array static data member
//meeting: Jacksonville 2/16
//edg_status: Passes
