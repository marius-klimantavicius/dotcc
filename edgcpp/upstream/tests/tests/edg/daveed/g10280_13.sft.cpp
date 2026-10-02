//remark:GNU attributes
//options:--gcc;cp:--g++;cp

struct S {
  unsigned int vmask __attribute__((aligned(8))); 
};

