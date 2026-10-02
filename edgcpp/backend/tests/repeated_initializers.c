struct Cell { int number; unsigned char byte; };
int integers[100000]={[0 ... 99999]=7};
struct Cell cells[20000]={[0 ... 19999]={13,17}};
char words[20000][4]={[0 ... 19999]="abc"};
int main(void) {
  if(sizeof(integers)!=400000 || sizeof(cells)!=160000 || sizeof(words)!=80000)return 1;
  for(int i=0;i<100000;++i)if(integers[i]!=7)return 2;
  for(int i=0;i<20000;++i)if(cells[i].number!=13 || cells[i].byte!=17 || words[i][0]!='a' || words[i][1]!='b' || words[i][2]!='c' || words[i][3])return 3;
  integers[99999]=91;cells[19999].byte=29;words[19999][2]='z';
  return integers[99998]==7 && cells[19998].byte==17 && words[19998][2]=='c' ? 0:4;
}
