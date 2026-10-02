//type:fc
//options_all:--c++20 --header_unit test.h=test.h.eifc --module_import_diagnostics -d-module_report

// The imported IFC file should be corrupted on disk resulting in a checksum error.
import "test.h";

int z = x;
