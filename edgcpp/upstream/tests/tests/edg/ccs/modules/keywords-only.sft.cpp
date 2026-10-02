//type:fn
//options:--c++17:--c++20:--microsoft_version 1928 --ms_c++latest:--c++20 --modules;fp
//options_all:-r --no_modules --set_flag skip_module_imports

export module bar;

import foo;

export int var = 3;
