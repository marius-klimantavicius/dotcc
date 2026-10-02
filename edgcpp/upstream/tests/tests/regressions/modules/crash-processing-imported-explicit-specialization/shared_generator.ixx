module;
#include "coroutine.h"

export module shared_gen;

export using handle_t = std::coroutine_handle<int>;
