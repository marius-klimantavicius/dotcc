//options_all:-r -x -tused
//options: --strict;cp

extern "C" int printf(char*, ...);
int main(void)
{
  int flag=0, x=1;
  if (!x)
    if (x) ;
      else ;
  else
    flag = 1;
  if (flag != 1)
    printf("error\n");
  else
    printf("ok\n");
  return 0;
}


