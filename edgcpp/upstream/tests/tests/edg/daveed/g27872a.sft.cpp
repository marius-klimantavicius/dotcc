//remark:Static call operator
//options:--ms_c++23 --microsoft_v=1944;fp:--ms_c++23 --microsoft_v=1943;fn:--c++23;fp

struct S {
  static int operator()();
};

static_assert(__cpp_static_call_operator == 202207L);
