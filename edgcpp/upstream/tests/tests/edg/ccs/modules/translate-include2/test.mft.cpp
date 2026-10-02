//type:fc
//options:;fp:--ms_translate_include:--ms_translate_include --no_ms_translate_include;fp
//options_all:-r --microsoft --modules --set_flag skip_module_version_check --ms_header_unit foo.h=foo.h.ifc
//source_files:foo.h

#include "foo.h"

void test() {
  foo();
}
