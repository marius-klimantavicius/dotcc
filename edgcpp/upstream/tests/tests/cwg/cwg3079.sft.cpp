//type:fp
//options:--c++14:--c++26
//options_all:-A -tused

struct C {
  union {
    int i;
    ;
  };

  union U {
    int i;
    ;
  };
};

//cwg: 3079
//title: Allow empty-declarations in anonymous unions
//meeting: Kona 11/25
//edg_status: Passes
