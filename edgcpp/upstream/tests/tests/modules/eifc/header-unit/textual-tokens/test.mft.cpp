//type:fp
//header_unit_files:test.h
//options_all:--module_import_diagnostics -d-module_report --c++20
import "test.h";

struct X {};

bool has_nothrow_assign = has_nothrow_assign_proxy<X>();
bool has_nothrow_constructor = has_nothrow_constructor_proxy<X>();
bool is_trivial = is_trivial_proxy<X>();
