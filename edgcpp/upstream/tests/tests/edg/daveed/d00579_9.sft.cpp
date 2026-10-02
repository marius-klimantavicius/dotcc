//remark:GNU C/C++ complex type support
//type:rp
//name:
//options:
//options_all:--g++
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:

extern "C" int printf(char const*, ...);

_Complex double z;

void f(double &) { printf("lvalue\n"); }
void f(double const&) { printf("rvalue\n"); }

int main() {
	f(__real z);
	f(__imag z);
	f(__real(z+z));
}
