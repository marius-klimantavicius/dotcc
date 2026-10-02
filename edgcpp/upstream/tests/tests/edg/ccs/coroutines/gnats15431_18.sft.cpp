//type:fn
//options:--microsoft_version=1900;fn:-DNEG --microsoft_version=1900;fn
//options_all:--set_flag coroutines -tused

void g() {
#ifdef NEG
  return;
#endif
  co_return;
}

