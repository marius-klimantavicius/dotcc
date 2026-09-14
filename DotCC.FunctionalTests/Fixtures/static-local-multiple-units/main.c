#include <stdio.h>
int left_tick(void);int right_tick(void);int left_record(void);int right_record(void);
int *left_buffer(void);int *right_buffer(void);int *left_alias(void);int *right_alias(void);
char *left_label(void);char *right_label(void);
int main(void){
  int *a=left_buffer(),*b=right_buffer();a[0]=7;b[1]=9;
  int left=left_tick(),right=right_tick();
  printf("ticks=%d,%d buffers=%d,%d,%d,%d distinct=%d\n",left,right,a[0],a[1],b[0],b[1],a!=b);
  printf("labels=%s,%s records=%d,%d aliases=%d,%d\n",left_label(),right_label(),left_record(),right_record(),left_alias()[0],right_alias()[1]);
  left=left_tick();right=right_tick();printf("retained=%d,%d\n",left,right);return 0;
}
