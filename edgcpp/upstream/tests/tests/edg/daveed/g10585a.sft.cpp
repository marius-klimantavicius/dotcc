//remark:[[final]] on class types
//options:--c++0x;fn:--c++0x -A;fn

struct [[ final ]] B2 {
  };

  struct D2 : B2 {    // ill-formed
  };
