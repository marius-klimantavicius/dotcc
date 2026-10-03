struct Pair { int first,second; };
static int calls;
static int next(void){return ++calls;}
#define SUM_ONCE(a,b) ({ int left=(a);int right=(b);left+right; })
#ifdef __cplusplus
extern "C"
#endif
int statement_expression_cases(void) {
 calls=0;
 int value=SUM_ONCE(next(),SUM_ONCE(next(),next()));
 if(value!=6 || calls!=3)return 1;
 int state=4;
 int result=({int total=0;for(int i=0;i!=5;++i){if(i==2)continue;total+=i;}total+state;});
 if(result!=12)return 2;
 result=({int total=0;int i=0;while(i!=7){++i;if(i==4)break;total+=i;}total;});
 if(result!=6)return 3;
 result=({int total=1;again: total*=2;if(total<16)goto again;total;});
 if(result!=16)return 4;
 result=({int total=0;switch(state){case 3:total=5;break;case 4:total=7;break;default:total=9;}total;});
 if(result!=7)return 5;
 struct Pair original={11,17};
 struct Pair copied=({struct Pair local=original;local.first+=2;local;});
 if(copied.first!=13 || copied.second!=17 || original.first!=11)return 6;
 if(0 && ({++calls;1;}))return 7;
 if(calls!=3)return 8;
 ({state+=3;(void)state;});
 if(state!=7)return 9;
#ifndef __cplusplus
 *({&state;})=19;
 if(state!=19)return 10;
#endif
 return 0;
}
