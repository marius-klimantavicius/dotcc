#include <stdlib.h>
#include <inttypes.h>
#include <stdint.h>
#include <errno.h>
#include <string.h>
struct number_result {uint64_t bits;unsigned signexp;int error;long consumed;};
#ifdef __cplusplus
extern "C"
#endif
void number_case(int kind,const char *input,int base,void *result) {
 struct number_result *out=(struct number_result*)result;
 char *end=(char*)input;errno=71;memset(out,0,sizeof *out);
 switch(kind) {
 case 0:out->bits=(uint64_t)strtol(input,&end,base);break;
 case 1:out->bits=(uint64_t)strtoul(input,&end,base);break;
 case 2:{float x=strtof(input,&end);unsigned bits;memcpy(&bits,&x,4);out->bits=bits;break;}
 case 3:{double x=strtod(input,&end);memcpy(&out->bits,&x,8);break;}
 case 4:{long double x=strtold(input,&end);unsigned short e;memcpy(&out->bits,&x,8);memcpy(&e,(const char*)&x+8,2);out->signexp=e;break;}
 case 5:out->bits=(uint64_t)atoi(input);break;
 case 6:out->bits=(uint64_t)atol(input);break;
 case 7:out->bits=(uint64_t)atoll(input);break;
 case 8:{double x=atof(input);memcpy(&out->bits,&x,8);break;}
 case 9:out->bits=(uint64_t)strtoimax(input,&end,base);break;
 case 10:out->bits=(uint64_t)strtoumax(input,&end,base);break;
 case 11:out->bits=(uint64_t)strtoll(input,&end,base);break;
 case 12:out->bits=(uint64_t)strtoull(input,&end,base);break;
 }
 out->error=errno;out->consumed=end-input;
}
