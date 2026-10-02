//type:fp
//header_unit_files:test.h
//options_all:--module_import_diagnostics -d-module_report --c++20
import "test.h";

int main(){
  if (return_true<bool>() != true) {
    return 1;
  }  /* if */
  if (return_false<bool>() != false) {
    return 1;
  }  /* if */
  return 0;
}
