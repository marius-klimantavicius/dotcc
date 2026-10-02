//remark:GNU C/C++ complex type support
//type:rp
//name:
//options:--gcc:--g++
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:

double d1 = __real(1.0+2.0i);
double d2 = __imag(1.0+2.0i);
__complex double z1 = 3.0+4.0j;

int main() {
  double d3 = __real z1;
  double d4 = __imag z1;
  __imag z1 = 3.0;
  double d5 = __imag(z1+z1);
  //__imag(z1+z1) = 3.0;

  return 0;
}
