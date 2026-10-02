//remark:Branch past initialization diagnostic
//type:fn
//name:
//options:-A:;fp
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:
class C {};
class D : public C {};

int main() {
  goto label_1;
  D d;
label_1:
  return 0;
}
