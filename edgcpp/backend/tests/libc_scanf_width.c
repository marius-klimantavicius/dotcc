/* Valid int-representable widths; tiny input never requires large storage. */
#include <stdio.h>
#include <wchar.h>
#ifndef NARROW_SCAN
#define NARROW_SCAN sscanf
#endif
#ifndef WIDE_SCAN
#define WIDE_SCAN swscanf
#endif
#ifdef ORIGINAL_NARROW
int original_scan(const char *,const char *,...);
#endif
#ifdef ORIGINAL_WIDE
int original_scan(const wchar_t *,const wchar_t *,...);
#endif
#ifdef __cplusplus
extern "C" {
#endif
#ifndef ORIGINAL_WIDE
int scan_width_narrow(void)
{
 static const char *formats[]={"%*2147483599c","%*2147483600c","%*2147483646c","%*2147483647c","%2147483599s%n","%2147483600s%n","%2147483646s%n","%2147483647s%n"};
 for(int i=0;i<8;++i){char output[2]={'?', '!'};int n=-1;int r=NARROW_SCAN("x",formats[i],output,&n);
  if(i<4){if(r!=0||output[0]!='?'||output[1]!='!'||n!=-1)return i+1;}
  else if(r!=1||output[0]!='x'||output[1]!=0||n!=1)return i+1;
 }
 return 0;
}
#endif
#ifndef ORIGINAL_NARROW
int scan_width_wide(void)
{
 static const wchar_t *formats[]={L"%*2147483599c",L"%*2147483600c",L"%*2147483646c",L"%*2147483647c",L"%2147483599s%n",L"%2147483600s%n",L"%2147483646s%n",L"%2147483647s%n"};
 for(int i=0;i<8;++i){char output[2]={'?', '!'};int n=-1;int r=WIDE_SCAN(L"x",formats[i],output,&n);
  if(i<4){if(r!=0||output[0]!='?'||output[1]!='!'||n!=-1)return i+1;}
  else if(r!=1||output[0]!='x'||output[1]!=0||n!=1)return i+1;
 }
 return 0;
}
#endif
#ifdef __cplusplus
}
#endif
