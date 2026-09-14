struct Cell { int value; };
typedef int Pair[2];
int left_tick(void){static int once=1;return ++once;}
int *left_buffer(void){static int buf[2];return buf;}
char *left_label(void){static char b[]="left";return b;}
int left_record(void){static struct Cell value={5};return ++value.value;}
int *left_alias(void){static Pair buf;buf[0]=11;buf[1]=12;return buf;}
