//remark:C++ VLAs
//type:fp
//name:
//options:
//options_all:--g++ -x
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:

//options_all:--c++ --vla --exceptions
// VLAs in C++, exception handling.
int seed = 0;
extern "C" int printf(const char *, ...);
struct A {
  int i;
  A() : i(++seed) { if (i == 15) { printf("throw\n"); throw 1; }
                    printf("construct %d\n", i);
                  }
  ~A() { printf("destroy   %d\n", i); }
};
int main() {
  int i = 10;
  {
    A x[i];
  }
  try {
    A y[i];
  } catch (...) {
    printf("catch\n");
  }
}

