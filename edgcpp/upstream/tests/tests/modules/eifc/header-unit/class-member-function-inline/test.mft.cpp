//type:rp
//header_unit_files:test.h
//options_all:--module_import_diagnostics -d-module_report --c++20
import "test.h";

int main() {
  return my_class::return_three() - (my_class{}.return_one() + my_class::return_two());
}
