static int calls;
static unsigned long next_value(void) { ++calls;return 7; }
int main(void) {
  unsigned long zero=0,one=1,high=1UL<<63;
  unsigned int small=3;
  unsigned __int128 wide=((unsigned __int128)1<<100)+9;
  if(-zero!=0 || -one!=~0UL || -high!=high)return 1;
  if(-small!=(unsigned int)0-3 || -wide!=(unsigned __int128)0-wide)return 2;
  if(-next_value()!=0UL-7 || calls!=1)return 3;
  return 0;
}
