//remark:C++ VLAs
//type:fp
//name:
//options:--g++;fn:--gcc;fp
//options_all:--gnu_version=40300
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:

int gn;

void f(int n, double a[n], double b[gn][n]) {
}
