//type:fp
//header_unit_files:test.h
//options_all:--module_import_diagnostics -d-module_report --c++20
import "test.h";

constexpr int return_value = return_zero();

static_assert(return_value == 0);
