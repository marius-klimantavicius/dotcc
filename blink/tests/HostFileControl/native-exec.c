#define _GNU_SOURCE 1
#include <errno.h>
#include <fcntl.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
int main(int argc, char **argv) {
  if (argc==4 && !strcmp(argv[1],"child")) {
    int closed=atoi(argv[2]),retained=atoi(argv[3]);
    if (fcntl(closed,F_GETFD)!=-1 || errno!=EBADF || fcntl(retained,F_GETFD)!=0) return 2;
    char data[5];
    if (read(retained,data,5)!=5 || memcmp(data,"alpha",5)) return 3;
    puts("native exec closes marked fd and retains ordinary duplicate: PASS");
    return 0;
  }
  int marked=open("data/image",O_RDONLY|O_CLOEXEC);
  if(marked<0)return 4;
  int retained=dup(marked);if(retained<0)return 5;
  char a[32],b[32];snprintf(a,sizeof(a),"%d",marked);snprintf(b,sizeof(b),"%d",retained);
  char *child[]={argv[0],"child",a,b,0};
  execv(argv[0],child);
  return 6;
}
