#define _POSIX_C_SOURCE 200809L
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <math.h>
#include <errno.h>
#ifdef DOTCC_TEST_GC
void ForceGc(void);
#else
static void ForceGc(void) {}
#endif
static unsigned long long bits(double value) { union DoubleBits {double d; unsigned long long u;}; union DoubleBits b; b.d=value; return b.u; }
static unsigned int fbits(float value) { union FloatBits {float f; unsigned int u;}; union FloatBits b; b.f=value; return b.u; }
static double frombits(unsigned long long value) { union MakeDouble {double d; unsigned long long u;}; union MakeDouble b; b.u=value; return b.d; }
int probe(void) {
  char output[16]; memset(output,0x5a,sizeof(output));
  char *cursor=stpcpy(output,"abc");
  if(cursor!=output+3 || *cursor || output[4]!=0x5a)return 1;
  if(stpcpy(cursor,"")!=cursor || stpcpy(cursor,"xyz")!=output+6 || strcmp(output,"abcxyz"))return 2;
  char source[5]={'a','b','c','d',0};
  errno=123;
  char *copy=strdup(source), *prefix=strndup(source,3), *empty=strndup(source,0);
  if(!copy || !prefix || !empty || copy==source || errno!=123)return 3;
  source[0]='X'; ForceGc();
  if(strcmp(copy,"abcd") || strcmp(prefix,"abc") || *empty)return 4;
  copy[1]='Y';if(source[1]!='b' || prefix[1]!='b')return 5;
  free(copy); free(prefix); free(empty);
  char unterminated[3]={'q','r','s'};
  prefix=strndup(unterminated,3); if(!prefix || strcmp(prefix,"qrs"))return 6;free(prefix);
  volatile size_t huge=(size_t)-1;
  prefix=strndup("short",huge);if(!prefix || strcmp(prefix,"short"))return 7;free(prefix);
  double inputs[]={-3.5,-2.5,-1.5,-0.5,-0.25,0.0,0.25,0.5,1.5,2.5,3.5,1.1,-1.1,4503599627370496.0};
  double expected[]={-4.0,-2.0,-2.0,-0.0,-0.0,0.0,0.0,0.0,2.0,2.0,4.0,1.0,-1.0,4503599627370496.0};
  double (*rounder)(double)=rint; float (*rounderf)(float)=rintf;
  for(int i=0;i<14;++i){
    if(bits(rounder(inputs[i]))!=bits(expected[i]))return 10+i;
    if(fbits(rounderf((float)inputs[i]))!=fbits((float)expected[i]))return 30+i;
  }
  double special[]={frombits(0x8000000000000000ULL),frombits(0x7ff0000000000000ULL),frombits(0xfff0000000000000ULL)};
  for(int i=0;i<3;++i)if(bits(rint(special[i]))!=bits(special[i]))return 50+i;
  double nan=frombits(0x7ff8000000000042ULL);
  if(!isnan(rint(nan)) || !isnan(rintf((float)nan)))return 54;
  int left=0,right=0;
  if(!isunordered((++left,nan),(++right,1.0)) || left!=1 || right!=1)return 55;
  if(!isunordered(1.0,nan) || !isunordered(nan,nan) || isunordered(-0.0,0.0) || isunordered(special[1],special[2]))return 56;
  if(errno!=123)return 57;
  return 0;
}
int main(void){int result=probe();if(result)return result;puts("Owned string copies and nearest rounding: PASS");return 0;}
