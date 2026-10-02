#include <stdio.h>
#include <stdint.h>
#include <limits.h>
static void sample(long counter,long frequency){
 long seconds=counter/frequency,remainder=counter%frequency;
 if(remainder<0){--seconds;remainder+=frequency;}
 printf("%ld %ld %ld %ld\n",counter,frequency,seconds,(long)((__int128)remainder*1000000000/frequency));
}
int main(void){
 long frequencies[]={1,3,7,10000000,1000000000,1000000001,LONG_MAX};
 long counters[]={LONG_MIN,LONG_MIN+1,-10000001,-10000000,-1000001,-1000000,-1,0,1,1000000,1000001,10000000,10000001,LONG_MAX-1,LONG_MAX};
 unsigned long random=0xfedcba9876543210UL;unsigned i,j;
 for(i=0;i<sizeof(counters)/sizeof(*counters);++i)for(j=0;j<sizeof(frequencies)/sizeof(*frequencies);++j)sample(counters[i],frequencies[j]);
 for(i=0;i<10000;++i){random=random*6364136223846793005UL+1442695040888963407UL;long counter=(long)random;random=random*6364136223846793005UL+1442695040888963407UL;sample(counter,(long)(random>>1)|1);}
 return 0;
}
