//type:fp
//options_all:--c++20 -r --set_flag skip_module_imports

module A:B;
import A:C;
import :C; // Duplicate import
