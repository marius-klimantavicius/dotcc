#include <errno.h>
#include <fcntl.h>
#include <poll.h>
#include <signal.h>
#include <stdio.h>
#include <string.h>
#include <sys/stat.h>
#include <sys/uio.h>
#include <unistd.h>
#ifdef BLINK_MANAGED_PIPES
#include "host-errors.h"
#include "host-io.h"
#include "host-pipes.h"
#endif
#define CHECK(x) do{if(!(x)){printf("pipe failure %d errno %d\n",__LINE__,errno);return __LINE__;}}while(0)
int PipeProbe(void) {
 int ends[2]={-7,-9};char buffer[8192];struct stat st;struct pollfd ready[2];
 CHECK(pipe2(ends,-1)==-1 && errno==EINVAL && ends[0]==-7 && ends[1]==-9);
 int (*create)(int*,int)=pipe2;
 CHECK(!create(ends,O_NONBLOCK|O_CLOEXEC));
 CHECK(fcntl(ends[0],F_GETFD)==FD_CLOEXEC && fcntl(ends[1],F_GETFD)==FD_CLOEXEC);
 CHECK((fcntl(ends[0],F_GETFL)&(O_ACCMODE|O_NONBLOCK))==O_NONBLOCK);
 CHECK((fcntl(ends[1],F_GETFL)&(O_ACCMODE|O_NONBLOCK))==(O_WRONLY|O_NONBLOCK));
 CHECK(!fstat(ends[0],&st) && S_ISFIFO(st.st_mode) && st.st_size==0 && st.st_nlink==1);
 CHECK(lseek(ends[0],0,SEEK_CUR)==-1 && errno==ESPIPE);
 CHECK(read(ends[0],buffer,1)==-1 && errno==EAGAIN);
 CHECK(write(ends[0],buffer,0)==-1 && errno==EBADF);
 ready[0]=(struct pollfd){ends[0],POLLIN,0};ready[1]=(struct pollfd){ends[1],POLLOUT,0};
 CHECK(poll(ready,2,0)==1 && !ready[0].revents && ready[1].revents==POLLOUT);
 struct iovec vectors[2]={{"abc",3},{"de",2}};
 CHECK(writev(ends[1],vectors,2)==5);
 CHECK(poll(ready,2,0)==2 && (ready[0].revents&POLLIN));
 struct iovec reads[2]={{buffer,2},{buffer+2,6}};
 CHECK(readv(ends[0],reads,2)==5 && !memcmp(buffer,"abcde",5));
 memset(buffer,'x',4000);long total=0;ssize_t written;
 while((written=write(ends[1],buffer,4000))>0){CHECK(written==4000);total+=written;CHECK(total<=1048576);}
 CHECK(written==-1 && errno==EAGAIN && total>=4000);
 CHECK(read(ends[0],buffer,4000)==4000);total-=4000;
 CHECK(write(ends[1],buffer,4000)==4000);total+=4000;
 long read_total=0;ssize_t count;
 while((count=read(ends[0],buffer,sizeof(buffer)))>0)read_total+=count;
 CHECK(count==-1 && errno==EAGAIN && read_total==total);
 int copy=dup(ends[1]);CHECK(copy>=0 && fcntl(copy,F_GETFD)==0);
 CHECK(!fcntl(copy,F_SETFL,0) && !(fcntl(ends[1],F_GETFL)&O_NONBLOCK));
 CHECK(!close(ends[1]));CHECK(read(ends[0],buffer,1)==-1 && errno==EAGAIN);
 CHECK(write(copy,"tail",4)==4 && !close(copy));
 ready[0]=(struct pollfd){ends[0],POLLIN,0};CHECK(poll(ready,1,0)==1 && (ready[0].revents&(POLLIN|POLLHUP))==(POLLIN|POLLHUP));
 CHECK(read(ends[0],buffer,sizeof(buffer))==4 && !memcmp(buffer,"tail",4));
 CHECK(read(ends[0],buffer,1)==0 && read(ends[0],buffer,0)==0);
 CHECK(!close(ends[0]));
 CHECK(!pipe(ends) && !close(ends[0]));
 CHECK(write(ends[1],buffer,0)==0 && write(ends[1],buffer,1)==-1 && errno==EPIPE);
 ready[0]=(struct pollfd){ends[1],POLLOUT,0};CHECK(poll(ready,1,0)==1 && ready[0].revents==(POLLOUT|POLLERR));
 CHECK(!close(ends[1]));puts("pipe common pass");return 0;
}
#ifdef BLINK_MANAGED_PIPES
int PipeFailure(int error) {int pair[2]={123,456};CHECK(pipe(pair)==-1 && errno==error && pair[0]==123 && pair[1]==456);return 0;}
int PipePrivate(void){CHECK(pipe(0)==-1 && errno==EFAULT);return 0;}
#else
int main(void){signal(SIGPIPE,SIG_IGN);return PipeProbe();}
#endif
