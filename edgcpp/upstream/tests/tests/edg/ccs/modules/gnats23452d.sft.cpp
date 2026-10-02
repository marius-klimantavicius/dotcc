//type:fn
//options_all:--c++20 --modules --set_flag skip_module_version_check

// No implicit import
module foo:part;

void impl() {
  test(); // Name not declared
}
