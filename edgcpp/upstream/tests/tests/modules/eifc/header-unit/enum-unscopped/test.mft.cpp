//type:cp
//header_unit_files:test.h
//options_all:--module_import_diagnostics -d-module_report --c++20
import "test.h";

static_assert(A == 0);
static_assert(B == 1);
static_assert(C == 10);
static_assert(D == 2'000'000'000);
