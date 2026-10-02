//remark:Block-extern in templates
//options:-A;cp:--gnu_version=30400;cp
//options_all:-tused

template<typename T> void h(void)
{
  extern T x[];
}

template<typename T> void g(void)
{
  extern T x[];
}

void f(void)
{
  g<int>();
}
