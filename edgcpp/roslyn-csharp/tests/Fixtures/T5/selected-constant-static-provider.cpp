#include "selected-constant-static.hpp"

extern "C" const char* selected_constant_b() noexcept { return selected_shared_constant(); }
extern "C" const char* selected_different_b() noexcept { return selected_different_constant(); }
