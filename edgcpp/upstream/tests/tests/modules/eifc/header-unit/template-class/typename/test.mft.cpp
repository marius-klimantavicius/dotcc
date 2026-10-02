//type:rp
//header_unit_files:test.h
//cases:3
//options_all:--module_import_diagnostics -d-module_report --c++20
import "test.h";

int main() {
  my_class<int> value(7);
  return value.templ_value - 7;
}
