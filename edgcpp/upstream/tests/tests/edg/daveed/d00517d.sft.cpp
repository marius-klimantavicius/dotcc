//remark:Positional format specifier
//type:fp
//name:
//options:
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:


#pragma __printf_args
int printf(char const*, ...);

int main() {
  int i = 5;
  printf("%*s\n",i,";");
  printf("%0$*1$s\n",i,";");
  printf("%1$*00$s\n",";",i);
  printf("Hello", i);
}
