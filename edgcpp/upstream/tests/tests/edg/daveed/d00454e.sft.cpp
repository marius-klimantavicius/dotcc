//remark:MSVC++ 7.1 for-init scope emulation
//type:rp
//name:
//options:
//options_all:--microsoft_version=1310 --wchar_t
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

#include <assert.h>
volatile int a;
int n=10;
int main()
{
int i = 0 ;
for (int i=0; i<n; i++) {
  a=i;
}
assert(i==0) ;
}
