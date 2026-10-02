//type:fp
//options:--clang_version 190100 --target linux_i686:--clang_version 190100 --target linux_x86_64:--clang_version 190100 --target linux_armv7:--clang_version 190100 --target linux_aarch64:--gn 140200 --target linux_i686:--gn 140200 --target linux_x86_64:--gn 140200 --target linux_armv7:--gn 140200 --target linux_aarch64
//options_all:-w --c++17 --il_display
//filter:awk '/^func-scope variable@/{print $0; f=1; next}/^$/{if (f) print $0; f=0}f' | grep -E -e '^(  name|  enclosing_routine|type):' -e '^func-scope ' -e '^$' | sed -e 's/@[0-9a-f]*//'

void f(__builtin_va_list) { }
