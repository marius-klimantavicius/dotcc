#include <fcntl.h>
#include <sys/file.h>
#include <unistd.h>
#include <errno.h>
#include <stdio.h>
#ifdef BLINK_PRIVATE_LOCKS
#include "host-errors.h"
#include "host-io.h"
#include "host-locks.h"
#endif
#define CHECK(x) do {if(!(x)){printf("locks line %d errno %d\n",__LINE__,errno);return __LINE__;}}while(0)
int LocksCommon(void) {
 int a=open("work",O_CREAT|O_RDWR|O_TRUNC,0600),b=open("work",O_RDWR);
 CHECK(a>=0 && b>=0);int c=dup(a);CHECK(c>=0);
 CHECK(!flock(a,LOCK_SH) && !flock(b,LOCK_SH));
 CHECK(flock(a,LOCK_EX|LOCK_NB)==-1 && errno==EWOULDBLOCK);
 CHECK(!flock(b,LOCK_UN) && !flock(b,LOCK_EX|LOCK_NB));
 CHECK(flock(a,LOCK_SH|LOCK_NB)==-1 && errno==EWOULDBLOCK);
 CHECK(!close(b) && !flock(a,LOCK_EX));
 CHECK(!flock(c,LOCK_UN));
 b=open("work",O_RDWR);CHECK(b>=0 && !flock(b,LOCK_EX|LOCK_NB));
 CHECK(!flock(b,LOCK_UN) && !flock(a,LOCK_SH));
 CHECK(!close(a));
 CHECK(flock(b,LOCK_EX|LOCK_NB)==-1 && errno==EWOULDBLOCK);
 CHECK(!close(c) && !flock(b,LOCK_EX|LOCK_NB));
 CHECK(flock(b,0)==-1 && errno==EINVAL);
 CHECK(flock(b,LOCK_SH|LOCK_EX)==-1 && errno==EINVAL);
 CHECK(!close(b));
 CHECK(flock(-999,LOCK_EX|LOCK_NB)==-1 && errno==EBADF);
 return 0;
}
#ifdef BLINK_PRIVATE_LOCKS
void LocksGc(void);
int LocksUnbound(int error) {
 CHECK(blink_io_flock(0,1)==-1 && errno==error);return 0;
}
int LocksPrivate(void) {
 int a=open("work",O_RDWR),b=open("work",O_RDWR);CHECK(a>=0 && b>=0);
 int (*operation)(int,int)=blink_io_flock;
 CHECK(!operation(a,LOCK_SH) && !operation(b,LOCK_SH));
 CHECK(operation(a,LOCK_EX)==-1 && errno==ENOTSUP);
 CHECK(!operation(b,LOCK_UN));
 CHECK(operation(b,LOCK_EX|LOCK_NB)==-1 && errno==EWOULDBLOCK);
 CHECK(!operation(a,LOCK_UN) && !operation(b,LOCK_EX|LOCK_NB));
 LocksGc();
 CHECK(operation(a,LOCK_SH|LOCK_NB)==-1 && errno==EWOULDBLOCK);
 CHECK(!close(a) && !close(b));
 CHECK(operation(0,LOCK_SH)==-1 && errno==ENOTSUP);
 return 0;
}
#else
int main(int argc,char **argv) {
 if(argc!=2 || chdir(argv[1]))return 2;
 if(LocksCommon())return 1;
 unlink("work");puts("private file locking common invariants: PASS");return 0;
}
#endif
