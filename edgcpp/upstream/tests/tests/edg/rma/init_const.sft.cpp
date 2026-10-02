//options_all:-r -x -tused
//options: --strict;cn

const int i;
static const int j;
extern const int k;
void f(void)
{
  const int i;
  static const int j;
  extern const int k;
  auto const int l;
  register const int m;
}

