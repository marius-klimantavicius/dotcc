struct Cell { int value; };
typedef int Pair[2];
int right_tick(void){static int once=100;return ++once;}
int *right_buffer(void){static int buf[2];return buf;}
char *right_label(void){static char b[]="right";return b;}
int right_record(void){static struct Cell value={50};return ++value.value;}
int *right_alias(void){static Pair buf;buf[0]=21;buf[1]=22;return buf;}
