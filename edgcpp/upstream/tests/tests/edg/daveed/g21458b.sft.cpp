//remark:vector types are trivially copyable
//options:--c++11 --g++;fp

  using V = int __attribute__((vector_size(4*sizeof(int))));
  static_assert(__is_trivially_copyable(V), "Unexpected");
    // Now accepted in GNU C++11 mode.
