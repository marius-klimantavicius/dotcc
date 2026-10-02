//type:rp
//header_unit_files:test.h
//options_all:--module_import_diagnostics -d-module_report --c++20
import "test.h";

template<typename T>
struct wrapper {
  wrapper(T value)
    : value(value)
  {}
  T value;
};


int main() {
  wrapper<int> wrapped_value(-10);
  return add_ten<wrapper>(wrapped_value);
}
