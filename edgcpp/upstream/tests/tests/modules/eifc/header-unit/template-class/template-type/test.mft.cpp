//type:rp
//header_unit_files:test.h
//cases:3
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
  wrapper<int>               wrapped_value(-10);
  add_ten_via_proxy<wrapper> value(wrapped_value);
  return value.wrapper.value;
}
