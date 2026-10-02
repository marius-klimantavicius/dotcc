//type:cp
//options_all:--c++20 --no_standard_includes --sys_include=$RUN_TEST_CURR_DIR/srcloc/inc --sys_include=$RUN_TEST_CURR_DIR/inc

#include <source_location>

enum resolution_type { non_const_void_ty, void_ty, non_const_impl_ty, impl_ty };

consteval resolution_type resolve(void *x) { return non_const_void_ty; }
consteval resolution_type resolve(const void *x) { return void_ty; }
consteval resolution_type resolve(std::source_location::__impl *x) { return non_const_impl_ty; }
consteval resolution_type resolve(const std::source_location::__impl *x) { return impl_ty; }

static_assert(resolve(__builtin_source_location()) == void_ty);
