//type:fp
//options:--microsoft_version=1900;fp
//options_all:--set_flag coroutines -tused

#include <coroutine>

struct X {
  X operator co_await() { return X(); }
};

X operator co_await(X) { return X(); }

