//type:cp
//header_unit_files:test.h
//options_all:--module_import_diagnostics -d-module_report --c++20
import "test.h";

constexpr auto value = my_class{1, 2, 3};

static_assert(value.a == 1);
static_assert(value.b == 2);
static_assert(value.c == 3);
