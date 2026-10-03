/* Synthetic IEEE representation oracle. Host GNU binary128/binary80 only. */
#include <stdint.h>
#include <stdio.h>
#include <string.h>
typedef union { __float128 value; uint64_t words[2]; } Quad;
static uint64_t state=0x1d897ba038463fe1ULL;
static uint64_t random_word(void) {state^=state<<13;state^=state>>7;state^=state<<17;return state;}
static void quad(Quad q) {
 if((q.words[1]&0x7fff000000000000ULL)==0x7fff000000000000ULL &&
    ((q.words[1]&0xffffffffffffULL)||q.words[0])) {q.words[1]=0x7fff800000000000ULL;q.words[0]=0;}
 printf("0x%016llxULL,0x%016llxULL,",(unsigned long long)q.words[0],(unsigned long long)q.words[1]);
}
int main(void) {
 static const uint64_t exponents[]={0,1,2,0x3ffe,0x3fff,0x4000,0x7ffd,0x7ffe,0x7fff};
 puts("static const unsigned long long float_rows[][16]={");
 for(unsigned i=0;i<2048;++i) {
  Quad a,b,r;unsigned expa,expb;
  if(i<81){expa=exponents[i/9];expb=exponents[i%9];}
  else if(i<1024){expa=16350+(random_word()%70);expb=16350+(random_word()%70);}
  else {expa=random_word()%32768;expb=random_word()%32768;}
  a.words[0]=random_word();b.words[0]=random_word();
  a.words[1]=(random_word()&0x8000ffffffffffffULL)|((uint64_t)expa<<48);
  b.words[1]=(random_word()&0x8000ffffffffffffULL)|((uint64_t)expb<<48);
  if(i<18){a.words[0]=b.words[0]=0;a.words[1]&=0xffff000000000000ULL;b.words[1]&=0xffff000000000000ULL;}
  printf("{");quad(a);quad(b);
  r.value=a.value+b.value;quad(r);r.value=a.value-b.value;quad(r);
  r.value=a.value*b.value;quad(r);r.value=a.value/b.value;quad(r);
  float f=(float)a.value;double d=(double)a.value;long double l=(long double)a.value;
  uint32_t fb;uint64_t db,ls;uint16_t le;
  memcpy(&fb,&f,4);memcpy(&db,&d,8);memcpy(&ls,&l,8);memcpy(&le,(char*)&l+8,2);
  if((fb&0x7f800000)==0x7f800000 && (fb&0x7fffff))fb=0x7fc00000;
  if((db&0x7ff0000000000000ULL)==0x7ff0000000000000ULL && (db&0xfffffffffffffULL))db=0x7ff8000000000000ULL;
  if((le&0x7fff)==0x7fff && ls!=0x8000000000000000ULL){ls=0xc000000000000000ULL;le=0x7fff;}
  printf("0x%08xULL,0x%016llxULL,0x%016llxULL,0x%04xULL},\n",fb,(unsigned long long)db,(unsigned long long)ls,le);
 }
 puts("};");return 0;
}
