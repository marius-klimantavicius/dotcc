//remark:GNU C attribute syntax
//type:cp
//name:
//options:;cp:-DFIX;cp
//options_all:--gcc
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

#ifdef FIX
void sane_get_select_fd (void *handle __attribute__((unused)),
                         int  *fd __attribute__((unused)))
#else
void sane_get_select_fd (void * __attribute__((unused)) handle,
                         int  * __attribute__((unused)) fd )
#endif
{
  return;
}

