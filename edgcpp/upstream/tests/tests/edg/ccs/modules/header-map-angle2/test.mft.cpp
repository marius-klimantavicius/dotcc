//type:fc
//options_all:-r --microsoft --modules --sys_include . --set_flag skip_module_version_check --ms_header_unit_angle foo.h=notfound.ifc
//source_files:foo.h

import <foo.h>;

void test() {
  foo();
}
