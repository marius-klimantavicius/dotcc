//type:cp
//header_unit_files:test.h
//options_all:--module_import_diagnostics -d-module_report --c++20
import "test.h";

int main() {
  my_namespace::my_class x;

  my_namespace::my_optional<int> opt_res = x.do_stuff('a', 10);
  if (!opt_res.has_value()) {
    return 0;
  }  /* if */

  my_namespace::point a = {1, 2, 3};
  my_namespace::point b = {1, 2, 3};
  my_namespace::point c = add_points(a, b);
  return *opt_res + c.x * c.y * c.z;
}
