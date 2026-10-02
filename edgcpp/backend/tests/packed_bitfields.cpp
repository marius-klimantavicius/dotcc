// Synthetic independent per-bit oracle for packed byte-wise field access.
struct __attribute__((packed)) T0 {unsigned char before;unsigned long long pad:1;unsigned long long value:1;unsigned long long tail:7;unsigned char after;};
static int check0() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T0 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<1)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<1)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<9;++bit) {
   int set=bit<1?1:bit<2?(int)((bits>>(bit-1))&1):(0x55>>(bit-2))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T0 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T1 {unsigned char before;unsigned long long pad:2;unsigned long long value:2;unsigned long long tail:7;unsigned char after;};
static int check1() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T1 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<2)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<2)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<11;++bit) {
   int set=bit<2?1:bit<4?(int)((bits>>(bit-2))&1):(0x55>>(bit-4))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T1 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T2 {unsigned char before;unsigned long long pad:3;unsigned long long value:3;unsigned long long tail:7;unsigned char after;};
static int check2() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T2 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<3)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<3)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<13;++bit) {
   int set=bit<3?1:bit<6?(int)((bits>>(bit-3))&1):(0x55>>(bit-6))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T2 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T3 {unsigned char before;unsigned long long pad:4;unsigned long long value:4;unsigned long long tail:7;unsigned char after;};
static int check3() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T3 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<4)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<4)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<15;++bit) {
   int set=bit<4?1:bit<8?(int)((bits>>(bit-4))&1):(0x55>>(bit-8))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T3 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T4 {unsigned char before;unsigned long long pad:5;unsigned long long value:5;unsigned long long tail:7;unsigned char after;};
static int check4() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T4 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<5)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<5)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<17;++bit) {
   int set=bit<5?1:bit<10?(int)((bits>>(bit-5))&1):(0x55>>(bit-10))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T4 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T5 {unsigned char before;unsigned long long pad:6;unsigned long long value:6;unsigned long long tail:7;unsigned char after;};
static int check5() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T5 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<6)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<6)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<19;++bit) {
   int set=bit<6?1:bit<12?(int)((bits>>(bit-6))&1):(0x55>>(bit-12))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T5 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T6 {unsigned char before;unsigned long long pad:7;unsigned long long value:7;unsigned long long tail:7;unsigned char after;};
static int check6() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T6 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<7)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<7)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<21;++bit) {
   int set=bit<7?1:bit<14?(int)((bits>>(bit-7))&1):(0x55>>(bit-14))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T6 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T7 {unsigned char before;unsigned long long value:8;unsigned long long tail:7;unsigned char after;};
static int check7() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T7 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<8)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<15;++bit) {
   int set=bit<0?1:bit<8?(int)((bits>>(bit-0))&1):(0x55>>(bit-8))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T7 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T8 {unsigned char before;unsigned long long pad:1;unsigned long long value:9;unsigned long long tail:7;unsigned char after;};
static int check8() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T8 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<1)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<9)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<17;++bit) {
   int set=bit<1?1:bit<10?(int)((bits>>(bit-1))&1):(0x55>>(bit-10))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T8 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T9 {unsigned char before;unsigned long long pad:2;unsigned long long value:10;unsigned long long tail:7;unsigned char after;};
static int check9() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T9 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<2)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<10)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<19;++bit) {
   int set=bit<2?1:bit<12?(int)((bits>>(bit-2))&1):(0x55>>(bit-12))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T9 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T10 {unsigned char before;unsigned long long pad:3;unsigned long long value:11;unsigned long long tail:7;unsigned char after;};
static int check10() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T10 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<3)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<11)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<21;++bit) {
   int set=bit<3?1:bit<14?(int)((bits>>(bit-3))&1):(0x55>>(bit-14))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T10 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T11 {unsigned char before;unsigned long long pad:4;unsigned long long value:12;unsigned long long tail:7;unsigned char after;};
static int check11() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T11 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<4)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<12)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<23;++bit) {
   int set=bit<4?1:bit<16?(int)((bits>>(bit-4))&1):(0x55>>(bit-16))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T11 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T12 {unsigned char before;unsigned long long pad:5;unsigned long long value:13;unsigned long long tail:7;unsigned char after;};
static int check12() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T12 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<5)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<13)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<25;++bit) {
   int set=bit<5?1:bit<18?(int)((bits>>(bit-5))&1):(0x55>>(bit-18))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T12 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T13 {unsigned char before;unsigned long long pad:6;unsigned long long value:14;unsigned long long tail:7;unsigned char after;};
static int check13() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T13 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<6)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<14)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<27;++bit) {
   int set=bit<6?1:bit<20?(int)((bits>>(bit-6))&1):(0x55>>(bit-20))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T13 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T14 {unsigned char before;unsigned long long pad:7;unsigned long long value:15;unsigned long long tail:7;unsigned char after;};
static int check14() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T14 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<7)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<15)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<29;++bit) {
   int set=bit<7?1:bit<22?(int)((bits>>(bit-7))&1):(0x55>>(bit-22))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T14 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T15 {unsigned char before;unsigned long long value:16;unsigned long long tail:7;unsigned char after;};
static int check15() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T15 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<16)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<23;++bit) {
   int set=bit<0?1:bit<16?(int)((bits>>(bit-0))&1):(0x55>>(bit-16))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T15 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T16 {unsigned char before;unsigned long long pad:1;unsigned long long value:17;unsigned long long tail:7;unsigned char after;};
static int check16() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T16 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<1)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<17)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<25;++bit) {
   int set=bit<1?1:bit<18?(int)((bits>>(bit-1))&1):(0x55>>(bit-18))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T16 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T17 {unsigned char before;unsigned long long pad:2;unsigned long long value:18;unsigned long long tail:7;unsigned char after;};
static int check17() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T17 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<2)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<18)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<27;++bit) {
   int set=bit<2?1:bit<20?(int)((bits>>(bit-2))&1):(0x55>>(bit-20))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T17 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T18 {unsigned char before;unsigned long long pad:3;unsigned long long value:19;unsigned long long tail:7;unsigned char after;};
static int check18() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T18 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<3)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<19)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<29;++bit) {
   int set=bit<3?1:bit<22?(int)((bits>>(bit-3))&1):(0x55>>(bit-22))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T18 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T19 {unsigned char before;unsigned long long pad:4;unsigned long long value:20;unsigned long long tail:7;unsigned char after;};
static int check19() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T19 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<4)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<20)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<31;++bit) {
   int set=bit<4?1:bit<24?(int)((bits>>(bit-4))&1):(0x55>>(bit-24))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T19 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T20 {unsigned char before;unsigned long long pad:5;unsigned long long value:21;unsigned long long tail:7;unsigned char after;};
static int check20() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T20 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<5)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<21)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<33;++bit) {
   int set=bit<5?1:bit<26?(int)((bits>>(bit-5))&1):(0x55>>(bit-26))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T20 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T21 {unsigned char before;unsigned long long pad:6;unsigned long long value:22;unsigned long long tail:7;unsigned char after;};
static int check21() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T21 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<6)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<22)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<35;++bit) {
   int set=bit<6?1:bit<28?(int)((bits>>(bit-6))&1):(0x55>>(bit-28))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T21 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T22 {unsigned char before;unsigned long long pad:7;unsigned long long value:23;unsigned long long tail:7;unsigned char after;};
static int check22() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T22 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<7)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<23)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<37;++bit) {
   int set=bit<7?1:bit<30?(int)((bits>>(bit-7))&1):(0x55>>(bit-30))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T22 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T23 {unsigned char before;unsigned long long value:24;unsigned long long tail:7;unsigned char after;};
static int check23() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T23 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<24)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<31;++bit) {
   int set=bit<0?1:bit<24?(int)((bits>>(bit-0))&1):(0x55>>(bit-24))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T23 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T24 {unsigned char before;unsigned long long pad:1;unsigned long long value:25;unsigned long long tail:7;unsigned char after;};
static int check24() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T24 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<1)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<25)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<33;++bit) {
   int set=bit<1?1:bit<26?(int)((bits>>(bit-1))&1):(0x55>>(bit-26))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T24 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T25 {unsigned char before;unsigned long long pad:2;unsigned long long value:26;unsigned long long tail:7;unsigned char after;};
static int check25() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T25 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<2)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<26)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<35;++bit) {
   int set=bit<2?1:bit<28?(int)((bits>>(bit-2))&1):(0x55>>(bit-28))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T25 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T26 {unsigned char before;unsigned long long pad:3;unsigned long long value:27;unsigned long long tail:7;unsigned char after;};
static int check26() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T26 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<3)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<27)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<37;++bit) {
   int set=bit<3?1:bit<30?(int)((bits>>(bit-3))&1):(0x55>>(bit-30))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T26 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T27 {unsigned char before;unsigned long long pad:4;unsigned long long value:28;unsigned long long tail:7;unsigned char after;};
static int check27() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T27 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<4)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<28)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<39;++bit) {
   int set=bit<4?1:bit<32?(int)((bits>>(bit-4))&1):(0x55>>(bit-32))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T27 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T28 {unsigned char before;unsigned long long pad:5;unsigned long long value:29;unsigned long long tail:7;unsigned char after;};
static int check28() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T28 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<5)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<29)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<41;++bit) {
   int set=bit<5?1:bit<34?(int)((bits>>(bit-5))&1):(0x55>>(bit-34))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T28 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T29 {unsigned char before;unsigned long long pad:6;unsigned long long value:30;unsigned long long tail:7;unsigned char after;};
static int check29() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T29 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<6)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<30)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<43;++bit) {
   int set=bit<6?1:bit<36?(int)((bits>>(bit-6))&1):(0x55>>(bit-36))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T29 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T30 {unsigned char before;unsigned long long pad:7;unsigned long long value:31;unsigned long long tail:7;unsigned char after;};
static int check30() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T30 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<7)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<31)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<45;++bit) {
   int set=bit<7?1:bit<38?(int)((bits>>(bit-7))&1):(0x55>>(bit-38))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T30 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T31 {unsigned char before;unsigned long long value:32;unsigned long long tail:7;unsigned char after;};
static int check31() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T31 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<32)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<39;++bit) {
   int set=bit<0?1:bit<32?(int)((bits>>(bit-0))&1):(0x55>>(bit-32))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T31 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T32 {unsigned char before;unsigned long long pad:1;unsigned long long value:33;unsigned long long tail:7;unsigned char after;};
static int check32() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T32 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<1)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<33)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<41;++bit) {
   int set=bit<1?1:bit<34?(int)((bits>>(bit-1))&1):(0x55>>(bit-34))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T32 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T33 {unsigned char before;unsigned long long pad:2;unsigned long long value:34;unsigned long long tail:7;unsigned char after;};
static int check33() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T33 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<2)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<34)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<43;++bit) {
   int set=bit<2?1:bit<36?(int)((bits>>(bit-2))&1):(0x55>>(bit-36))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T33 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T34 {unsigned char before;unsigned long long pad:3;unsigned long long value:35;unsigned long long tail:7;unsigned char after;};
static int check34() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T34 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<3)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<35)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<45;++bit) {
   int set=bit<3?1:bit<38?(int)((bits>>(bit-3))&1):(0x55>>(bit-38))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T34 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T35 {unsigned char before;unsigned long long pad:4;unsigned long long value:36;unsigned long long tail:7;unsigned char after;};
static int check35() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T35 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<4)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<36)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<47;++bit) {
   int set=bit<4?1:bit<40?(int)((bits>>(bit-4))&1):(0x55>>(bit-40))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T35 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T36 {unsigned char before;unsigned long long pad:5;unsigned long long value:37;unsigned long long tail:7;unsigned char after;};
static int check36() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T36 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<5)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<37)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<49;++bit) {
   int set=bit<5?1:bit<42?(int)((bits>>(bit-5))&1):(0x55>>(bit-42))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T36 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T37 {unsigned char before;unsigned long long pad:6;unsigned long long value:38;unsigned long long tail:7;unsigned char after;};
static int check37() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T37 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<6)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<38)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<51;++bit) {
   int set=bit<6?1:bit<44?(int)((bits>>(bit-6))&1):(0x55>>(bit-44))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T37 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T38 {unsigned char before;unsigned long long pad:7;unsigned long long value:39;unsigned long long tail:7;unsigned char after;};
static int check38() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T38 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<7)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<39)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<53;++bit) {
   int set=bit<7?1:bit<46?(int)((bits>>(bit-7))&1):(0x55>>(bit-46))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T38 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T39 {unsigned char before;unsigned long long value:40;unsigned long long tail:7;unsigned char after;};
static int check39() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T39 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<40)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<47;++bit) {
   int set=bit<0?1:bit<40?(int)((bits>>(bit-0))&1):(0x55>>(bit-40))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T39 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T40 {unsigned char before;unsigned long long pad:1;unsigned long long value:41;unsigned long long tail:7;unsigned char after;};
static int check40() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T40 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<1)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<41)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<49;++bit) {
   int set=bit<1?1:bit<42?(int)((bits>>(bit-1))&1):(0x55>>(bit-42))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T40 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T41 {unsigned char before;unsigned long long pad:2;unsigned long long value:42;unsigned long long tail:7;unsigned char after;};
static int check41() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T41 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<2)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<42)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<51;++bit) {
   int set=bit<2?1:bit<44?(int)((bits>>(bit-2))&1):(0x55>>(bit-44))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T41 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T42 {unsigned char before;unsigned long long pad:3;unsigned long long value:43;unsigned long long tail:7;unsigned char after;};
static int check42() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T42 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<3)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<43)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<53;++bit) {
   int set=bit<3?1:bit<46?(int)((bits>>(bit-3))&1):(0x55>>(bit-46))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T42 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T43 {unsigned char before;unsigned long long pad:4;unsigned long long value:44;unsigned long long tail:7;unsigned char after;};
static int check43() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T43 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<4)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<44)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<55;++bit) {
   int set=bit<4?1:bit<48?(int)((bits>>(bit-4))&1):(0x55>>(bit-48))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T43 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T44 {unsigned char before;unsigned long long pad:5;unsigned long long value:45;unsigned long long tail:7;unsigned char after;};
static int check44() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T44 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<5)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<45)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<57;++bit) {
   int set=bit<5?1:bit<50?(int)((bits>>(bit-5))&1):(0x55>>(bit-50))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T44 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T45 {unsigned char before;unsigned long long pad:6;unsigned long long value:46;unsigned long long tail:7;unsigned char after;};
static int check45() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T45 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<6)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<46)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<59;++bit) {
   int set=bit<6?1:bit<52?(int)((bits>>(bit-6))&1):(0x55>>(bit-52))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T45 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T46 {unsigned char before;unsigned long long pad:7;unsigned long long value:47;unsigned long long tail:7;unsigned char after;};
static int check46() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T46 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<7)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<47)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<61;++bit) {
   int set=bit<7?1:bit<54?(int)((bits>>(bit-7))&1):(0x55>>(bit-54))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T46 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T47 {unsigned char before;unsigned long long value:48;unsigned long long tail:7;unsigned char after;};
static int check47() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T47 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<48)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<55;++bit) {
   int set=bit<0?1:bit<48?(int)((bits>>(bit-0))&1):(0x55>>(bit-48))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T47 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T48 {unsigned char before;unsigned long long pad:1;unsigned long long value:49;unsigned long long tail:7;unsigned char after;};
static int check48() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T48 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<1)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<49)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<57;++bit) {
   int set=bit<1?1:bit<50?(int)((bits>>(bit-1))&1):(0x55>>(bit-50))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T48 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T49 {unsigned char before;unsigned long long pad:2;unsigned long long value:50;unsigned long long tail:7;unsigned char after;};
static int check49() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T49 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<2)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<50)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<59;++bit) {
   int set=bit<2?1:bit<52?(int)((bits>>(bit-2))&1):(0x55>>(bit-52))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T49 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T50 {unsigned char before;unsigned long long pad:3;unsigned long long value:51;unsigned long long tail:7;unsigned char after;};
static int check50() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T50 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<3)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<51)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<61;++bit) {
   int set=bit<3?1:bit<54?(int)((bits>>(bit-3))&1):(0x55>>(bit-54))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T50 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T51 {unsigned char before;unsigned long long pad:4;unsigned long long value:52;unsigned long long tail:7;unsigned char after;};
static int check51() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T51 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<4)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<52)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<63;++bit) {
   int set=bit<4?1:bit<56?(int)((bits>>(bit-4))&1):(0x55>>(bit-56))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T51 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T52 {unsigned char before;unsigned long long pad:5;unsigned long long value:53;unsigned long long tail:7;unsigned char after;};
static int check52() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T52 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<5)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<53)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<65;++bit) {
   int set=bit<5?1:bit<58?(int)((bits>>(bit-5))&1):(0x55>>(bit-58))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T52 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T53 {unsigned char before;unsigned long long pad:6;unsigned long long value:54;unsigned long long tail:7;unsigned char after;};
static int check53() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T53 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<6)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<54)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<67;++bit) {
   int set=bit<6?1:bit<60?(int)((bits>>(bit-6))&1):(0x55>>(bit-60))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T53 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T54 {unsigned char before;unsigned long long pad:7;unsigned long long value:55;unsigned long long tail:7;unsigned char after;};
static int check54() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T54 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<7)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<55)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<69;++bit) {
   int set=bit<7?1:bit<62?(int)((bits>>(bit-7))&1):(0x55>>(bit-62))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T54 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T55 {unsigned char before;unsigned long long value:56;unsigned long long tail:7;unsigned char after;};
static int check55() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T55 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<56)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<63;++bit) {
   int set=bit<0?1:bit<56?(int)((bits>>(bit-0))&1):(0x55>>(bit-56))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T55 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T56 {unsigned char before;unsigned long long pad:1;unsigned long long value:57;unsigned long long tail:7;unsigned char after;};
static int check56() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T56 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<1)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<57)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<65;++bit) {
   int set=bit<1?1:bit<58?(int)((bits>>(bit-1))&1):(0x55>>(bit-58))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T56 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T57 {unsigned char before;unsigned long long pad:2;unsigned long long value:58;unsigned long long tail:7;unsigned char after;};
static int check57() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T57 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<2)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<58)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<67;++bit) {
   int set=bit<2?1:bit<60?(int)((bits>>(bit-2))&1):(0x55>>(bit-60))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T57 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T58 {unsigned char before;unsigned long long pad:3;unsigned long long value:59;unsigned long long tail:7;unsigned char after;};
static int check58() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T58 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<3)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<59)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<69;++bit) {
   int set=bit<3?1:bit<62?(int)((bits>>(bit-3))&1):(0x55>>(bit-62))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T58 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T59 {unsigned char before;unsigned long long pad:4;unsigned long long value:60;unsigned long long tail:7;unsigned char after;};
static int check59() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T59 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<4)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<60)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<71;++bit) {
   int set=bit<4?1:bit<64?(int)((bits>>(bit-4))&1):(0x55>>(bit-64))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T59 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T60 {unsigned char before;unsigned long long pad:5;unsigned long long value:61;unsigned long long tail:7;unsigned char after;};
static int check60() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T60 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<5)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<61)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<73;++bit) {
   int set=bit<5?1:bit<66?(int)((bits>>(bit-5))&1):(0x55>>(bit-66))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T60 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T61 {unsigned char before;unsigned long long pad:6;unsigned long long value:62;unsigned long long tail:7;unsigned char after;};
static int check61() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T61 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<6)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<62)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<75;++bit) {
   int set=bit<6?1:bit<68?(int)((bits>>(bit-6))&1):(0x55>>(bit-68))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T61 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T62 {unsigned char before;unsigned long long pad:7;unsigned long long value:63;unsigned long long tail:7;unsigned char after;};
static int check62() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T62 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<7)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<63)-1);
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<77;++bit) {
   int set=bit<7?1:bit<70?(int)((bits>>(bit-7))&1):(0x55>>(bit-70))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T62 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T63 {unsigned char before;unsigned long long value:64;unsigned long long tail:7;unsigned char after;};
static int check63() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T63 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&~0ULL;
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<71;++bit) {
   int set=bit<0?1:bit<64?(int)((bits>>(bit-0))&1):(0x55>>(bit-64))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T63 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T64 {unsigned char before;unsigned long long pad:1;unsigned long long value:64;unsigned long long tail:7;unsigned char after;};
static int check64() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T64 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<1)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&~0ULL;
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<72;++bit) {
   int set=bit<1?1:bit<65?(int)((bits>>(bit-1))&1):(0x55>>(bit-65))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T64 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T65 {unsigned char before;unsigned long long pad:2;unsigned long long value:64;unsigned long long tail:7;unsigned char after;};
static int check65() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T65 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<2)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&~0ULL;
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<73;++bit) {
   int set=bit<2?1:bit<66?(int)((bits>>(bit-2))&1):(0x55>>(bit-66))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T65 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T66 {unsigned char before;unsigned long long pad:3;unsigned long long value:64;unsigned long long tail:7;unsigned char after;};
static int check66() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T66 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<3)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&~0ULL;
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<74;++bit) {
   int set=bit<3?1:bit<67?(int)((bits>>(bit-3))&1):(0x55>>(bit-67))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T66 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T67 {unsigned char before;unsigned long long pad:4;unsigned long long value:64;unsigned long long tail:7;unsigned char after;};
static int check67() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T67 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<4)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&~0ULL;
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<75;++bit) {
   int set=bit<4?1:bit<68?(int)((bits>>(bit-4))&1):(0x55>>(bit-68))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T67 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T68 {unsigned char before;unsigned long long pad:5;unsigned long long value:64;unsigned long long tail:7;unsigned char after;};
static int check68() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T68 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<5)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&~0ULL;
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<76;++bit) {
   int set=bit<5?1:bit<69?(int)((bits>>(bit-5))&1):(0x55>>(bit-69))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T68 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T69 {unsigned char before;unsigned long long pad:6;unsigned long long value:64;unsigned long long tail:7;unsigned char after;};
static int check69() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T69 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<6)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&~0ULL;
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<77;++bit) {
   int set=bit<6?1:bit<70?(int)((bits>>(bit-6))&1):(0x55>>(bit-70))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T69 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T70 {unsigned char before;unsigned long long pad:7;unsigned long long value:64;unsigned long long tail:7;unsigned char after;};
static int check70() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T70 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<7)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&~0ULL;
  object.value=(unsigned long long)bits;
  if(object.value!=bits)return 1;
  for(int bit=0;bit<78;++bit) {
   int set=bit<7?1:bit<71?(int)((bits>>(bit-7))&1):(0x55>>(bit-71))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T70 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T71 {unsigned char before;unsigned long long pad:1;signed long long value:1;unsigned long long tail:7;unsigned char after;};
static int check71() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T71 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<1)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<1)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 63) >> 63))return 1;
  for(int bit=0;bit<9;++bit) {
   int set=bit<1?1:bit<2?(int)((bits>>(bit-1))&1):(0x55>>(bit-2))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T71 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T72 {unsigned char before;unsigned long long pad:2;signed long long value:2;unsigned long long tail:7;unsigned char after;};
static int check72() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T72 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<2)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<2)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 62) >> 62))return 1;
  for(int bit=0;bit<11;++bit) {
   int set=bit<2?1:bit<4?(int)((bits>>(bit-2))&1):(0x55>>(bit-4))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T72 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T73 {unsigned char before;unsigned long long pad:3;signed long long value:3;unsigned long long tail:7;unsigned char after;};
static int check73() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T73 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<3)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<3)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 61) >> 61))return 1;
  for(int bit=0;bit<13;++bit) {
   int set=bit<3?1:bit<6?(int)((bits>>(bit-3))&1):(0x55>>(bit-6))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T73 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T74 {unsigned char before;unsigned long long pad:4;signed long long value:4;unsigned long long tail:7;unsigned char after;};
static int check74() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T74 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<4)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<4)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 60) >> 60))return 1;
  for(int bit=0;bit<15;++bit) {
   int set=bit<4?1:bit<8?(int)((bits>>(bit-4))&1):(0x55>>(bit-8))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T74 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T75 {unsigned char before;unsigned long long pad:5;signed long long value:5;unsigned long long tail:7;unsigned char after;};
static int check75() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T75 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<5)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<5)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 59) >> 59))return 1;
  for(int bit=0;bit<17;++bit) {
   int set=bit<5?1:bit<10?(int)((bits>>(bit-5))&1):(0x55>>(bit-10))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T75 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T76 {unsigned char before;unsigned long long pad:6;signed long long value:6;unsigned long long tail:7;unsigned char after;};
static int check76() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T76 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<6)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<6)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 58) >> 58))return 1;
  for(int bit=0;bit<19;++bit) {
   int set=bit<6?1:bit<12?(int)((bits>>(bit-6))&1):(0x55>>(bit-12))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T76 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T77 {unsigned char before;unsigned long long pad:7;signed long long value:7;unsigned long long tail:7;unsigned char after;};
static int check77() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T77 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<7)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<7)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 57) >> 57))return 1;
  for(int bit=0;bit<21;++bit) {
   int set=bit<7?1:bit<14?(int)((bits>>(bit-7))&1):(0x55>>(bit-14))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T77 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T78 {unsigned char before;signed long long value:8;unsigned long long tail:7;unsigned char after;};
static int check78() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T78 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<8)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 56) >> 56))return 1;
  for(int bit=0;bit<15;++bit) {
   int set=bit<0?1:bit<8?(int)((bits>>(bit-0))&1):(0x55>>(bit-8))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T78 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T79 {unsigned char before;unsigned long long pad:1;signed long long value:9;unsigned long long tail:7;unsigned char after;};
static int check79() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T79 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<1)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<9)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 55) >> 55))return 1;
  for(int bit=0;bit<17;++bit) {
   int set=bit<1?1:bit<10?(int)((bits>>(bit-1))&1):(0x55>>(bit-10))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T79 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T80 {unsigned char before;unsigned long long pad:2;signed long long value:10;unsigned long long tail:7;unsigned char after;};
static int check80() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T80 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<2)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<10)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 54) >> 54))return 1;
  for(int bit=0;bit<19;++bit) {
   int set=bit<2?1:bit<12?(int)((bits>>(bit-2))&1):(0x55>>(bit-12))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T80 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T81 {unsigned char before;unsigned long long pad:3;signed long long value:11;unsigned long long tail:7;unsigned char after;};
static int check81() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T81 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<3)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<11)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 53) >> 53))return 1;
  for(int bit=0;bit<21;++bit) {
   int set=bit<3?1:bit<14?(int)((bits>>(bit-3))&1):(0x55>>(bit-14))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T81 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T82 {unsigned char before;unsigned long long pad:4;signed long long value:12;unsigned long long tail:7;unsigned char after;};
static int check82() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T82 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<4)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<12)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 52) >> 52))return 1;
  for(int bit=0;bit<23;++bit) {
   int set=bit<4?1:bit<16?(int)((bits>>(bit-4))&1):(0x55>>(bit-16))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T82 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T83 {unsigned char before;unsigned long long pad:5;signed long long value:13;unsigned long long tail:7;unsigned char after;};
static int check83() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T83 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<5)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<13)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 51) >> 51))return 1;
  for(int bit=0;bit<25;++bit) {
   int set=bit<5?1:bit<18?(int)((bits>>(bit-5))&1):(0x55>>(bit-18))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T83 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T84 {unsigned char before;unsigned long long pad:6;signed long long value:14;unsigned long long tail:7;unsigned char after;};
static int check84() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T84 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<6)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<14)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 50) >> 50))return 1;
  for(int bit=0;bit<27;++bit) {
   int set=bit<6?1:bit<20?(int)((bits>>(bit-6))&1):(0x55>>(bit-20))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T84 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T85 {unsigned char before;unsigned long long pad:7;signed long long value:15;unsigned long long tail:7;unsigned char after;};
static int check85() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T85 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<7)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<15)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 49) >> 49))return 1;
  for(int bit=0;bit<29;++bit) {
   int set=bit<7?1:bit<22?(int)((bits>>(bit-7))&1):(0x55>>(bit-22))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T85 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T86 {unsigned char before;signed long long value:16;unsigned long long tail:7;unsigned char after;};
static int check86() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T86 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<16)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 48) >> 48))return 1;
  for(int bit=0;bit<23;++bit) {
   int set=bit<0?1:bit<16?(int)((bits>>(bit-0))&1):(0x55>>(bit-16))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T86 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T87 {unsigned char before;unsigned long long pad:1;signed long long value:17;unsigned long long tail:7;unsigned char after;};
static int check87() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T87 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<1)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<17)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 47) >> 47))return 1;
  for(int bit=0;bit<25;++bit) {
   int set=bit<1?1:bit<18?(int)((bits>>(bit-1))&1):(0x55>>(bit-18))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T87 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T88 {unsigned char before;unsigned long long pad:2;signed long long value:18;unsigned long long tail:7;unsigned char after;};
static int check88() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T88 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<2)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<18)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 46) >> 46))return 1;
  for(int bit=0;bit<27;++bit) {
   int set=bit<2?1:bit<20?(int)((bits>>(bit-2))&1):(0x55>>(bit-20))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T88 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T89 {unsigned char before;unsigned long long pad:3;signed long long value:19;unsigned long long tail:7;unsigned char after;};
static int check89() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T89 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<3)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<19)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 45) >> 45))return 1;
  for(int bit=0;bit<29;++bit) {
   int set=bit<3?1:bit<22?(int)((bits>>(bit-3))&1):(0x55>>(bit-22))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T89 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T90 {unsigned char before;unsigned long long pad:4;signed long long value:20;unsigned long long tail:7;unsigned char after;};
static int check90() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T90 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<4)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<20)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 44) >> 44))return 1;
  for(int bit=0;bit<31;++bit) {
   int set=bit<4?1:bit<24?(int)((bits>>(bit-4))&1):(0x55>>(bit-24))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T90 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T91 {unsigned char before;unsigned long long pad:5;signed long long value:21;unsigned long long tail:7;unsigned char after;};
static int check91() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T91 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<5)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<21)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 43) >> 43))return 1;
  for(int bit=0;bit<33;++bit) {
   int set=bit<5?1:bit<26?(int)((bits>>(bit-5))&1):(0x55>>(bit-26))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T91 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T92 {unsigned char before;unsigned long long pad:6;signed long long value:22;unsigned long long tail:7;unsigned char after;};
static int check92() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T92 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<6)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<22)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 42) >> 42))return 1;
  for(int bit=0;bit<35;++bit) {
   int set=bit<6?1:bit<28?(int)((bits>>(bit-6))&1):(0x55>>(bit-28))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T92 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T93 {unsigned char before;unsigned long long pad:7;signed long long value:23;unsigned long long tail:7;unsigned char after;};
static int check93() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T93 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<7)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<23)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 41) >> 41))return 1;
  for(int bit=0;bit<37;++bit) {
   int set=bit<7?1:bit<30?(int)((bits>>(bit-7))&1):(0x55>>(bit-30))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T93 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T94 {unsigned char before;signed long long value:24;unsigned long long tail:7;unsigned char after;};
static int check94() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T94 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<24)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 40) >> 40))return 1;
  for(int bit=0;bit<31;++bit) {
   int set=bit<0?1:bit<24?(int)((bits>>(bit-0))&1):(0x55>>(bit-24))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T94 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T95 {unsigned char before;unsigned long long pad:1;signed long long value:25;unsigned long long tail:7;unsigned char after;};
static int check95() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T95 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<1)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<25)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 39) >> 39))return 1;
  for(int bit=0;bit<33;++bit) {
   int set=bit<1?1:bit<26?(int)((bits>>(bit-1))&1):(0x55>>(bit-26))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T95 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T96 {unsigned char before;unsigned long long pad:2;signed long long value:26;unsigned long long tail:7;unsigned char after;};
static int check96() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T96 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<2)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<26)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 38) >> 38))return 1;
  for(int bit=0;bit<35;++bit) {
   int set=bit<2?1:bit<28?(int)((bits>>(bit-2))&1):(0x55>>(bit-28))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T96 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T97 {unsigned char before;unsigned long long pad:3;signed long long value:27;unsigned long long tail:7;unsigned char after;};
static int check97() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T97 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<3)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<27)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 37) >> 37))return 1;
  for(int bit=0;bit<37;++bit) {
   int set=bit<3?1:bit<30?(int)((bits>>(bit-3))&1):(0x55>>(bit-30))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T97 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T98 {unsigned char before;unsigned long long pad:4;signed long long value:28;unsigned long long tail:7;unsigned char after;};
static int check98() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T98 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<4)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<28)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 36) >> 36))return 1;
  for(int bit=0;bit<39;++bit) {
   int set=bit<4?1:bit<32?(int)((bits>>(bit-4))&1):(0x55>>(bit-32))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T98 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T99 {unsigned char before;unsigned long long pad:5;signed long long value:29;unsigned long long tail:7;unsigned char after;};
static int check99() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T99 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<5)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<29)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 35) >> 35))return 1;
  for(int bit=0;bit<41;++bit) {
   int set=bit<5?1:bit<34?(int)((bits>>(bit-5))&1):(0x55>>(bit-34))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T99 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T100 {unsigned char before;unsigned long long pad:6;signed long long value:30;unsigned long long tail:7;unsigned char after;};
static int check100() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T100 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<6)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<30)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 34) >> 34))return 1;
  for(int bit=0;bit<43;++bit) {
   int set=bit<6?1:bit<36?(int)((bits>>(bit-6))&1):(0x55>>(bit-36))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T100 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T101 {unsigned char before;unsigned long long pad:7;signed long long value:31;unsigned long long tail:7;unsigned char after;};
static int check101() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T101 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<7)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<31)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 33) >> 33))return 1;
  for(int bit=0;bit<45;++bit) {
   int set=bit<7?1:bit<38?(int)((bits>>(bit-7))&1):(0x55>>(bit-38))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T101 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T102 {unsigned char before;signed long long value:32;unsigned long long tail:7;unsigned char after;};
static int check102() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T102 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<32)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 32) >> 32))return 1;
  for(int bit=0;bit<39;++bit) {
   int set=bit<0?1:bit<32?(int)((bits>>(bit-0))&1):(0x55>>(bit-32))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T102 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T103 {unsigned char before;unsigned long long pad:1;signed long long value:33;unsigned long long tail:7;unsigned char after;};
static int check103() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T103 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<1)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<33)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 31) >> 31))return 1;
  for(int bit=0;bit<41;++bit) {
   int set=bit<1?1:bit<34?(int)((bits>>(bit-1))&1):(0x55>>(bit-34))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T103 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T104 {unsigned char before;unsigned long long pad:2;signed long long value:34;unsigned long long tail:7;unsigned char after;};
static int check104() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T104 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<2)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<34)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 30) >> 30))return 1;
  for(int bit=0;bit<43;++bit) {
   int set=bit<2?1:bit<36?(int)((bits>>(bit-2))&1):(0x55>>(bit-36))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T104 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T105 {unsigned char before;unsigned long long pad:3;signed long long value:35;unsigned long long tail:7;unsigned char after;};
static int check105() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T105 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<3)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<35)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 29) >> 29))return 1;
  for(int bit=0;bit<45;++bit) {
   int set=bit<3?1:bit<38?(int)((bits>>(bit-3))&1):(0x55>>(bit-38))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T105 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T106 {unsigned char before;unsigned long long pad:4;signed long long value:36;unsigned long long tail:7;unsigned char after;};
static int check106() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T106 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<4)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<36)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 28) >> 28))return 1;
  for(int bit=0;bit<47;++bit) {
   int set=bit<4?1:bit<40?(int)((bits>>(bit-4))&1):(0x55>>(bit-40))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T106 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T107 {unsigned char before;unsigned long long pad:5;signed long long value:37;unsigned long long tail:7;unsigned char after;};
static int check107() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T107 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<5)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<37)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 27) >> 27))return 1;
  for(int bit=0;bit<49;++bit) {
   int set=bit<5?1:bit<42?(int)((bits>>(bit-5))&1):(0x55>>(bit-42))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T107 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T108 {unsigned char before;unsigned long long pad:6;signed long long value:38;unsigned long long tail:7;unsigned char after;};
static int check108() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T108 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<6)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<38)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 26) >> 26))return 1;
  for(int bit=0;bit<51;++bit) {
   int set=bit<6?1:bit<44?(int)((bits>>(bit-6))&1):(0x55>>(bit-44))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T108 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T109 {unsigned char before;unsigned long long pad:7;signed long long value:39;unsigned long long tail:7;unsigned char after;};
static int check109() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T109 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<7)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<39)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 25) >> 25))return 1;
  for(int bit=0;bit<53;++bit) {
   int set=bit<7?1:bit<46?(int)((bits>>(bit-7))&1):(0x55>>(bit-46))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T109 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T110 {unsigned char before;signed long long value:40;unsigned long long tail:7;unsigned char after;};
static int check110() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T110 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<40)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 24) >> 24))return 1;
  for(int bit=0;bit<47;++bit) {
   int set=bit<0?1:bit<40?(int)((bits>>(bit-0))&1):(0x55>>(bit-40))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T110 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T111 {unsigned char before;unsigned long long pad:1;signed long long value:41;unsigned long long tail:7;unsigned char after;};
static int check111() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T111 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<1)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<41)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 23) >> 23))return 1;
  for(int bit=0;bit<49;++bit) {
   int set=bit<1?1:bit<42?(int)((bits>>(bit-1))&1):(0x55>>(bit-42))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T111 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T112 {unsigned char before;unsigned long long pad:2;signed long long value:42;unsigned long long tail:7;unsigned char after;};
static int check112() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T112 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<2)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<42)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 22) >> 22))return 1;
  for(int bit=0;bit<51;++bit) {
   int set=bit<2?1:bit<44?(int)((bits>>(bit-2))&1):(0x55>>(bit-44))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T112 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T113 {unsigned char before;unsigned long long pad:3;signed long long value:43;unsigned long long tail:7;unsigned char after;};
static int check113() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T113 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<3)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<43)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 21) >> 21))return 1;
  for(int bit=0;bit<53;++bit) {
   int set=bit<3?1:bit<46?(int)((bits>>(bit-3))&1):(0x55>>(bit-46))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T113 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T114 {unsigned char before;unsigned long long pad:4;signed long long value:44;unsigned long long tail:7;unsigned char after;};
static int check114() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T114 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<4)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<44)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 20) >> 20))return 1;
  for(int bit=0;bit<55;++bit) {
   int set=bit<4?1:bit<48?(int)((bits>>(bit-4))&1):(0x55>>(bit-48))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T114 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T115 {unsigned char before;unsigned long long pad:5;signed long long value:45;unsigned long long tail:7;unsigned char after;};
static int check115() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T115 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<5)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<45)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 19) >> 19))return 1;
  for(int bit=0;bit<57;++bit) {
   int set=bit<5?1:bit<50?(int)((bits>>(bit-5))&1):(0x55>>(bit-50))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T115 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T116 {unsigned char before;unsigned long long pad:6;signed long long value:46;unsigned long long tail:7;unsigned char after;};
static int check116() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T116 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<6)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<46)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 18) >> 18))return 1;
  for(int bit=0;bit<59;++bit) {
   int set=bit<6?1:bit<52?(int)((bits>>(bit-6))&1):(0x55>>(bit-52))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T116 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T117 {unsigned char before;unsigned long long pad:7;signed long long value:47;unsigned long long tail:7;unsigned char after;};
static int check117() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T117 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<7)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<47)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 17) >> 17))return 1;
  for(int bit=0;bit<61;++bit) {
   int set=bit<7?1:bit<54?(int)((bits>>(bit-7))&1):(0x55>>(bit-54))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T117 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T118 {unsigned char before;signed long long value:48;unsigned long long tail:7;unsigned char after;};
static int check118() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T118 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<48)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 16) >> 16))return 1;
  for(int bit=0;bit<55;++bit) {
   int set=bit<0?1:bit<48?(int)((bits>>(bit-0))&1):(0x55>>(bit-48))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T118 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T119 {unsigned char before;unsigned long long pad:1;signed long long value:49;unsigned long long tail:7;unsigned char after;};
static int check119() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T119 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<1)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<49)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 15) >> 15))return 1;
  for(int bit=0;bit<57;++bit) {
   int set=bit<1?1:bit<50?(int)((bits>>(bit-1))&1):(0x55>>(bit-50))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T119 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T120 {unsigned char before;unsigned long long pad:2;signed long long value:50;unsigned long long tail:7;unsigned char after;};
static int check120() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T120 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<2)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<50)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 14) >> 14))return 1;
  for(int bit=0;bit<59;++bit) {
   int set=bit<2?1:bit<52?(int)((bits>>(bit-2))&1):(0x55>>(bit-52))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T120 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T121 {unsigned char before;unsigned long long pad:3;signed long long value:51;unsigned long long tail:7;unsigned char after;};
static int check121() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T121 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<3)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<51)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 13) >> 13))return 1;
  for(int bit=0;bit<61;++bit) {
   int set=bit<3?1:bit<54?(int)((bits>>(bit-3))&1):(0x55>>(bit-54))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T121 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T122 {unsigned char before;unsigned long long pad:4;signed long long value:52;unsigned long long tail:7;unsigned char after;};
static int check122() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T122 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<4)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<52)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 12) >> 12))return 1;
  for(int bit=0;bit<63;++bit) {
   int set=bit<4?1:bit<56?(int)((bits>>(bit-4))&1):(0x55>>(bit-56))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T122 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T123 {unsigned char before;unsigned long long pad:5;signed long long value:53;unsigned long long tail:7;unsigned char after;};
static int check123() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T123 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<5)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<53)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 11) >> 11))return 1;
  for(int bit=0;bit<65;++bit) {
   int set=bit<5?1:bit<58?(int)((bits>>(bit-5))&1):(0x55>>(bit-58))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T123 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T124 {unsigned char before;unsigned long long pad:6;signed long long value:54;unsigned long long tail:7;unsigned char after;};
static int check124() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T124 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<6)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<54)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 10) >> 10))return 1;
  for(int bit=0;bit<67;++bit) {
   int set=bit<6?1:bit<60?(int)((bits>>(bit-6))&1):(0x55>>(bit-60))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T124 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T125 {unsigned char before;unsigned long long pad:7;signed long long value:55;unsigned long long tail:7;unsigned char after;};
static int check125() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T125 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<7)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<55)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 9) >> 9))return 1;
  for(int bit=0;bit<69;++bit) {
   int set=bit<7?1:bit<62?(int)((bits>>(bit-7))&1):(0x55>>(bit-62))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T125 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T126 {unsigned char before;signed long long value:56;unsigned long long tail:7;unsigned char after;};
static int check126() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T126 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<56)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 8) >> 8))return 1;
  for(int bit=0;bit<63;++bit) {
   int set=bit<0?1:bit<56?(int)((bits>>(bit-0))&1):(0x55>>(bit-56))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T126 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T127 {unsigned char before;unsigned long long pad:1;signed long long value:57;unsigned long long tail:7;unsigned char after;};
static int check127() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T127 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<1)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<57)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 7) >> 7))return 1;
  for(int bit=0;bit<65;++bit) {
   int set=bit<1?1:bit<58?(int)((bits>>(bit-1))&1):(0x55>>(bit-58))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T127 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T128 {unsigned char before;unsigned long long pad:2;signed long long value:58;unsigned long long tail:7;unsigned char after;};
static int check128() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T128 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<2)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<58)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 6) >> 6))return 1;
  for(int bit=0;bit<67;++bit) {
   int set=bit<2?1:bit<60?(int)((bits>>(bit-2))&1):(0x55>>(bit-60))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T128 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T129 {unsigned char before;unsigned long long pad:3;signed long long value:59;unsigned long long tail:7;unsigned char after;};
static int check129() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T129 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<3)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<59)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 5) >> 5))return 1;
  for(int bit=0;bit<69;++bit) {
   int set=bit<3?1:bit<62?(int)((bits>>(bit-3))&1):(0x55>>(bit-62))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T129 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T130 {unsigned char before;unsigned long long pad:4;signed long long value:60;unsigned long long tail:7;unsigned char after;};
static int check130() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T130 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<4)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<60)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 4) >> 4))return 1;
  for(int bit=0;bit<71;++bit) {
   int set=bit<4?1:bit<64?(int)((bits>>(bit-4))&1):(0x55>>(bit-64))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T130 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T131 {unsigned char before;unsigned long long pad:5;signed long long value:61;unsigned long long tail:7;unsigned char after;};
static int check131() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T131 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<5)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<61)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 3) >> 3))return 1;
  for(int bit=0;bit<73;++bit) {
   int set=bit<5?1:bit<66?(int)((bits>>(bit-5))&1):(0x55>>(bit-66))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T131 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T132 {unsigned char before;unsigned long long pad:6;signed long long value:62;unsigned long long tail:7;unsigned char after;};
static int check132() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T132 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<6)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<62)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 2) >> 2))return 1;
  for(int bit=0;bit<75;++bit) {
   int set=bit<6?1:bit<68?(int)((bits>>(bit-6))&1):(0x55>>(bit-68))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T132 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T133 {unsigned char before;unsigned long long pad:7;signed long long value:63;unsigned long long tail:7;unsigned char after;};
static int check133() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T133 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<7)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&((1ULL<<63)-1);
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 1) >> 1))return 1;
  for(int bit=0;bit<77;++bit) {
   int set=bit<7?1:bit<70?(int)((bits>>(bit-7))&1):(0x55>>(bit-70))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T133 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T134 {unsigned char before;signed long long value:64;unsigned long long tail:7;unsigned char after;};
static int check134() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T134 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&~0ULL;
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 0) >> 0))return 1;
  for(int bit=0;bit<71;++bit) {
   int set=bit<0?1:bit<64?(int)((bits>>(bit-0))&1):(0x55>>(bit-64))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T134 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T135 {unsigned char before;unsigned long long pad:1;signed long long value:64;unsigned long long tail:7;unsigned char after;};
static int check135() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T135 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<1)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&~0ULL;
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 0) >> 0))return 1;
  for(int bit=0;bit<72;++bit) {
   int set=bit<1?1:bit<65?(int)((bits>>(bit-1))&1):(0x55>>(bit-65))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T135 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T136 {unsigned char before;unsigned long long pad:2;signed long long value:64;unsigned long long tail:7;unsigned char after;};
static int check136() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T136 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<2)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&~0ULL;
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 0) >> 0))return 1;
  for(int bit=0;bit<73;++bit) {
   int set=bit<2?1:bit<66?(int)((bits>>(bit-2))&1):(0x55>>(bit-66))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T136 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T137 {unsigned char before;unsigned long long pad:3;signed long long value:64;unsigned long long tail:7;unsigned char after;};
static int check137() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T137 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<3)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&~0ULL;
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 0) >> 0))return 1;
  for(int bit=0;bit<74;++bit) {
   int set=bit<3?1:bit<67?(int)((bits>>(bit-3))&1):(0x55>>(bit-67))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T137 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T138 {unsigned char before;unsigned long long pad:4;signed long long value:64;unsigned long long tail:7;unsigned char after;};
static int check138() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T138 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<4)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&~0ULL;
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 0) >> 0))return 1;
  for(int bit=0;bit<75;++bit) {
   int set=bit<4?1:bit<68?(int)((bits>>(bit-4))&1):(0x55>>(bit-68))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T138 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T139 {unsigned char before;unsigned long long pad:5;signed long long value:64;unsigned long long tail:7;unsigned char after;};
static int check139() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T139 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<5)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&~0ULL;
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 0) >> 0))return 1;
  for(int bit=0;bit<76;++bit) {
   int set=bit<5?1:bit<69?(int)((bits>>(bit-5))&1):(0x55>>(bit-69))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T139 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T140 {unsigned char before;unsigned long long pad:6;signed long long value:64;unsigned long long tail:7;unsigned char after;};
static int check140() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T140 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<6)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&~0ULL;
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 0) >> 0))return 1;
  for(int bit=0;bit<77;++bit) {
   int set=bit<6?1:bit<70?(int)((bits>>(bit-6))&1):(0x55>>(bit-70))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T140 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
struct __attribute__((packed)) T141 {unsigned char before;unsigned long long pad:7;signed long long value:64;unsigned long long tail:7;unsigned char after;};
static int check141() {
 unsigned long long seed=0x9e3779b97f4a7c15ULL;
 for(int iteration=0;iteration<32;++iteration) {
  T141 object={};unsigned char expected[sizeof(object)]={};
  object.before=0xa5;object.after=0x5a;expected[0]=0xa5;expected[sizeof(object)-1]=0x5a;
  object.pad=(1U<<7)-1;object.tail=0x55;
  seed^=seed<<13;seed^=seed>>7;seed^=seed<<17;
  unsigned long long bits=(iteration==0?0ULL:iteration==1?~0ULL:iteration==2?1ULL:iteration==3?0x8000000000000000ULL:seed)&~0ULL;
  object.value=(signed long long)bits;
  if(object.value!=((long long)(bits << 0) >> 0))return 1;
  for(int bit=0;bit<78;++bit) {
   int set=bit<7?1:bit<71?(int)((bits>>(bit-7))&1):(0x55>>(bit-71))&1;
   if(set)expected[1+bit/8]|=(unsigned char)(1U<<(bit%8));
  }
  for(unsigned long byte=0;byte<sizeof(object);++byte)if(((unsigned char*)&object)[byte]!=expected[byte])return 2;
  T141 copy=object;if(copy.value!=object.value || copy.tail!=0x55)return 3;
 }return 0;
}
int main(){return check0() || check1() || check2() || check3() || check4() || check5() || check6() || check7() || check8() || check9() || check10() || check11() || check12() || check13() || check14() || check15() || check16() || check17() || check18() || check19() || check20() || check21() || check22() || check23() || check24() || check25() || check26() || check27() || check28() || check29() || check30() || check31() || check32() || check33() || check34() || check35() || check36() || check37() || check38() || check39() || check40() || check41() || check42() || check43() || check44() || check45() || check46() || check47() || check48() || check49() || check50() || check51() || check52() || check53() || check54() || check55() || check56() || check57() || check58() || check59() || check60() || check61() || check62() || check63() || check64() || check65() || check66() || check67() || check68() || check69() || check70() || check71() || check72() || check73() || check74() || check75() || check76() || check77() || check78() || check79() || check80() || check81() || check82() || check83() || check84() || check85() || check86() || check87() || check88() || check89() || check90() || check91() || check92() || check93() || check94() || check95() || check96() || check97() || check98() || check99() || check100() || check101() || check102() || check103() || check104() || check105() || check106() || check107() || check108() || check109() || check110() || check111() || check112() || check113() || check114() || check115() || check116() || check117() || check118() || check119() || check120() || check121() || check122() || check123() || check124() || check125() || check126() || check127() || check128() || check129() || check130() || check131() || check132() || check133() || check134() || check135() || check136() || check137() || check138() || check139() || check140() || check141();}
