/* GNU C++ VLA object lifetimes, failed construction and handler cleanup. */
static int next_id, live, destroyed, throw_at=-1;
static unsigned long long order;
struct Item {
 int id;
 Item():id(next_id++) {if(id==throw_at)throw id;++live;}
 ~Item(){--live;++destroyed;order=order*10+id;}
};
static void create(int n,bool fail_body) {
 Item items[n];
 if(live!=n)throw 91;
 if(fail_body)throw 77;
}
int main() {
 next_id=live=destroyed=order=0;throw_at=-1;
 create(3,false);
 if(live || destroyed!=3 || order!=210)return 1;
 next_id=live=destroyed=order=0;
 try {create(4,true);return 2;}catch(int value){if(value!=77)return 3;}
 if(live || destroyed!=4 || order!=3210)return 4;
 next_id=live=destroyed=order=0;throw_at=2;
 try {create(5,false);return 5;}catch(int value){if(value!=2)return 6;}
 if(live || destroyed!=2 || order!=10)return 7;
 throw_at=-1;
 for(int iteration=0;iteration<12;++iteration) {
  next_id=live=destroyed=order=0;
  Item outer[iteration+1];
  {int n=2;Item inner[n];if(live!=iteration+3)return 8;}
  if(live!=iteration+1)return 9;
 }
 return live!=0;
}
