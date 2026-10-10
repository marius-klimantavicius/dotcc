extern "C" void _Exit(int);
int fail=0,live=0,event_count=0;
char events[24];
static void record(char value){events[event_count++]=value;}
static bool same(const char* expected){for(int i=0;expected[i]||events[i];++i)if(expected[i]!=events[i])return false;return true;}
static void clear(){event_count=0;for(int i=0;i<24;++i)events[i]=0;}
struct Report {~Report(){if(live!=0||!same("dcba"))_Exit(70);}} report;
struct Member {int value;Member(int n):value(n){record(char('A'+n-1));if(n==fail)throw n;++live;}~Member(){record(char('a'+value-1));--live;}};
struct Row {int tag;Member first;Member second;};
Row rows[]={{7,Member(1),Member(2)},{9,Member(3),Member(4)}};
int run(int failure){fail=failure;try{Row local[]={{7,Member(1),Member(2)},{9,Member(3),Member(4)}};if(failure||local[1].second.value!=4)return 1;}catch(int value){if(value!=failure)return 2;}return 0;}
int main(){if(live!=4||!same("ABCD"))_Exit(1);for(int mode=0;mode<=4;++mode){clear();if(run(mode)||live!=4)_Exit(2);const char* expected=mode==0?"ABCDdcba":mode==1?"A":mode==2?"ABa":mode==3?"ABCba":"ABCDcba";if(!same(expected))_Exit(3);}clear();return 0;}
