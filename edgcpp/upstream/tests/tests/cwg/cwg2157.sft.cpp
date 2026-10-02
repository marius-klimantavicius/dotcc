//type:fp
//options_all:--c++17 -tused -A
//
  enum E : int;
  struct X {
    enum ::E : int();
  };

//cwg: 2157
//title: Further disambiguation of enumeration elaborated-type-specifier
//meeting: Jacksonville 2/16
//edg_status: Passes
