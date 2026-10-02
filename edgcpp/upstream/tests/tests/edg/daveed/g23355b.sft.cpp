//remark:Value-initialization checks
//options:--c++11;fn

  struct S { union { int const ic; } u; };
  S x = S();
  S y = S{};

