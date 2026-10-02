//type:cp
//options::--gnu_version 70500;fn
//options_all:--c++14 --target linux_x86_64

struct __va_list_tag {
  unsigned int gp_offset;
  unsigned int fp_offset;
  char *overflow_arg_area;
  char *reg_save_area;
};
