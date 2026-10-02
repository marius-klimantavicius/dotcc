//remark:Union/nonunion mismatch for type_info
//type:fn
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

namespace std {
  class type_info {
    union type_info& operator=(const type_info&);  
  };
}
   
