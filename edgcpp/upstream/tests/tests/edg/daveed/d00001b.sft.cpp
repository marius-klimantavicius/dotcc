/*
//remark:Ordinary designators
//type:rp
//name:
//options:
//options_all:--c --designators
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
*/

#if defined __cplusplus
extern "C"
#endif
int printf(char const*, ...);

typedef int Table[20][40];

void print_table(Table b) {
  int k, l;
  for (k = 0; k<6; ++k) {
    for (l = 0; l<6; ++l) {
      printf("%4d ", b[k][l]);
    }
    printf("\n");
  }
  printf("\n");
}


int x[20][40] = {
   [1] = { 7, 8, 9 },
   [3][4] = 11,
   [5][4] = { 22 }
};

void f() {
  int x[20][40] = {
     [4] = { 7, 8, 9 },
     [1][4] = 11,
     [5][2] = { 22 }
  };
  print_table(x);
}

void g() {
  int x[20][40] = {
     [2] = { 7, 8, 9 },
     [2][4] = 11,
     [2][2] = { 22 }
  };
  print_table(x);
}

int main() {
  print_table(x);
  f();
  g();
  return 0;
}

