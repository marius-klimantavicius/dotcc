//type:fc
//options_all:--c++20 --modules --set_flag skip_module_version_check

// Implicitly imports foo
module foo;

void impl() {
  test();
}
