//type:cp
//header_unit_files:test.h
//options_all:--module_import_diagnostics -d-module_report --c++20
import "test.h";

using namespace baz;

int return_bar() {
  return get_value();
}
