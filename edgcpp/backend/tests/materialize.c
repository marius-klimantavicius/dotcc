/* Array subobjects of aggregate results need storage until the call returns.
   The aligned local must not overlap backend-created result storage. */
typedef struct Record { unsigned char Buffer[41]; int Stamp; } Record;
Record make_record(int value) {
  Record result;
  for (int i=0;i<41;++i) result.Buffer[i]=(unsigned char)(value+i);
  result.Stamp=value+91;
  return result;
}
static int inspect(const unsigned char* a,const unsigned char* b,
                   unsigned* ordinary,unsigned char* aligned,void (*hook)(void)) {
  volatile unsigned occupied[256];
  for(int i=0;i<256;++i)occupied[i]=(unsigned)i+701;
  hook();
  if(a==b || a[0]!=7 || a[40]!=47 || b[0]!=13 || b[40]!=53)return -1;
  if(ordinary)for(int i=0;i<17;++i)if(ordinary[i]!=(unsigned)(0x531000+i))return -2;
  if(aligned) {
    if((unsigned long long)aligned%64)return -3;
    for(int i=0;i<128;++i)if(aligned[i]!=(unsigned char)(191+i))return -4;
  }
  return a[12]+b[23]+(int)occupied[255]-956;
}
int run_materialize_c(int test,void(*hook)(void)) {
  int (*callback)(const unsigned char*,const unsigned char*,unsigned*,unsigned char*,void(*)(void))=inspect;
  if(test==0)return callback(make_record(7).Buffer,make_record(13).Buffer,0,0,hook);
  if(test==1) {
    unsigned ordinary[17];
    for(int i=0;i<17;++i)ordinary[i]=(unsigned)(0x531000+i);
    int result=callback(make_record(7).Buffer,make_record(13).Buffer,ordinary,0,hook);
    for(int i=0;i<17;++i)if(ordinary[i]!=(unsigned)(0x531000+i))return -5;
    return result;
  }
  if(test==2) {
    unsigned ordinary[17];
    __attribute__((aligned(64))) unsigned char aligned[128];
    for(int i=0;i<17;++i)ordinary[i]=(unsigned)(0x531000+i);
    for(int i=0;i<128;++i)aligned[i]=(unsigned char)(191+i);
    int result=0;
    for(int iteration=0;iteration<4;++iteration)
      result+=callback(make_record(7).Buffer,make_record(13).Buffer,ordinary,aligned,hook);
    for(int i=0;i<17;++i)if(ordinary[i]!=(unsigned)(0x531000+i))return -6;
    for(int i=0;i<128;++i)if(aligned[i]!=(unsigned char)(191+i))return -7;
    return result;
  }
  if(test==3) {
    int side=0;
    int result=callback((side+=5,side+=7,make_record(7)).Buffer,make_record(13).Buffer,0,0,hook);
    return result+side*100;
  }
  if(test==4) {
    int side=0,other=0;
    int result=callback((side==0?(side+=11,make_record(7)):make_record(8)).Buffer,
                        make_record(13).Buffer,0,0,(other+=23,hook));
    return result+side*10000+other*100;
  }
  return -99;
}
