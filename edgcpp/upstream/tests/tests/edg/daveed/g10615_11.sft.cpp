//remark:Move and copy operations
//options:--c++11 -A;fp:--c++11 -A -DNEG;fn

struct MO {
  MO(MO const&&);
};

struct M {
  MO const m;
  M();
};

M x;
M y(static_cast<M&&>(x));
#ifdef NEG
M z(static_cast<M const&&>(x));
#endif
