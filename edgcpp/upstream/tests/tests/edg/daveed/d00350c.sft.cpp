//remark:Microsoft for-init scopes
//type:rp
//name:
//options:;rp:-DPOS=1 --new_for_init;rp:--old_for_init;rp
//options_all:--microsoft_version=1310 --wchar_t
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

#include <assert.h>
#ifndef POS
#define POS 0
#endif

int main() {
    int i = -1;
    {
        for (int i = 0; i < 10; ++i) ;
        assert( (i == 10) ^ POS);                    // with /Zc, i value is -1
    }
}
