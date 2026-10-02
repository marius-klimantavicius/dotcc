//type:fp
//header_unit_files:test.h
//options_all:--module_import_diagnostics -d-module_report --c++20
import "test.h";

constexpr int return_value = my_class::return_three() - (my_class{}.return_one() + my_class::return_two());

static_assert(return_value == 0);
