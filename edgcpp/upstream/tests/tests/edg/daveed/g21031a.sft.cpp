//remark:GNU constant folding
//options:--gcc --c11;fp:--c11;fn

  _Static_assert((int*)(void*)0 == (int*)0, "");  // Accepted in GNU C mode.
