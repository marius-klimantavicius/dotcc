unsigned long* report;
char** saved_arguments;
int saved_count;
#ifdef __cplusplus
extern "C"
#endif
void set_report(unsigned long* value) { report=value; }
#ifdef __cplusplus
struct CheckAtExit {
  ~CheckAtExit() {
    if(report) {
      unsigned long length=0;
      if(saved_count)while(saved_arguments[0][length])++length;
      *report=1000*length+(saved_count?(unsigned char)saved_arguments[0][0]:0);
    }
  }
} check_at_exit;
#endif
int main(int count,char** arguments) {
  saved_arguments=arguments;saved_count=count;
  if(arguments[count])return 1;
  if(!count)return 13;
  if(!arguments[0] || !arguments[0][0])return 2;
  if(count!=4)return 3;
  const unsigned char expected[]={0xce,0xb1,0xf0,0x9f,0x99,0x82,0};
  for(int i=0;i<7;++i)if((unsigned char)arguments[1][i]!=expected[i])return 4;
  if(arguments[2][0])return 5;
  if(arguments[3][0]!='t'||arguments[3][1]!='a'||arguments[3][2]!='i'||arguments[3][3]!='l'||arguments[3][4])return 6;
  arguments[0][0]='P';arguments[3][0]='T';
  return 0;
}
