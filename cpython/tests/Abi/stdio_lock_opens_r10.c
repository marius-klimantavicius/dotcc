/* Real stdio/descriptor services; independent native peer in the Python test. */
#define _POSIX_C_SOURCE 200809L
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <fcntl.h>
#include <unistd.h>
#include <sys/file.h>
#include <errno.h>

static int checks;
#define CHECK(x) do { checks++; if (!(x)) { fprintf(stderr,"failed check=%d line=%d errno=%d\n",checks,__LINE__,errno); return 1; } } while(0)
static void phase(const char *kind, const char *path) {
    printf("phase %s %s\n", kind, path); fflush(stdout);
    (void)getchar();
}
static int probe(const char *path, const char *templ) {
    FILE *f = fopen(path,"r+");
    if (!f) return 2;
    phase("fopen",path);
    if (fclose(f)) return 3;
    f = fopen(path,"r+");
    if (!f || freopen(path,"r+",f) != f) return 4;
    phase("freopen",path);
    if (fclose(f)) return 5;
    char name[4096]; strcpy(name,templ);
    int fd = mkstemp(name);
    if (fd < 0) return 6;
    phase("mkstemp",name);
    if (close(fd)) return 7;
    phase("closed",name);
    if (unlink(name)) return 8;
    puts("probe complete"); fflush(stdout);
    return 0;
}
static int contract(const char *path, const char *templ) {
    /* The peer owns LOCK_EX while fopen/freopen run. Neither open may lock. */
    phase("peer-owned",path);
    FILE *f = fopen(path,"r+"); CHECK(f != NULL);
    CHECK(freopen(path,"r+",f) == f);
    phase("opened-under-lock",path);
    CHECK(fclose(f) == 0);
    phase("peer-release",path);
    f = fopen(path,"r+"); CHECK(f != NULL);
    int fd = fileno(f); CHECK(fd >= 3);
    CHECK(flock(fd,LOCK_EX | LOCK_NB) == 0);
    phase("explicit",path);
    FILE *other = fopen(path,"r"); CHECK(other != NULL);
    CHECK(fclose(other) == 0);
    phase("explicit-after-other-close",path);
    CHECK(fclose(f) == 0);
    phase("explicit-closed",path);
    /* Distinct classic process locks survive flock queries, but another close
       of the same inode releases them, as required by fcntl rather than flock. */
    f = fopen(path,"r+"); CHECK(f != NULL); fd = fileno(f);
    struct flock range = {0}; range.l_type=F_WRLCK; range.l_start=2; range.l_len=3;
    CHECK(fcntl(fd,F_SETLK,&range) == 0);
    phase("record",path);
    other=fopen(path,"r"); CHECK(other != NULL && fclose(other)==0);
    phase("record-other-close",path);
    CHECK(fclose(f)==0);
    /* Native append remains writable after seeks; a+ is also readable. */
    f=fopen(path,"w+"); CHECK(f != NULL);
    CHECK(fwrite("abc",1,3,f)==3 && fclose(f)==0);
    f=fopen(path,"a+"); CHECK(f != NULL);
    CHECK(fseek(f,0,SEEK_SET)==0 && fgetc(f)=='a');
    CHECK(fseek(f,0,SEEK_SET)==0 && fputc('Z',f)=='Z' && fflush(f)==0);
    CHECK(ftell(f)==4 && fseek(f,0,SEEK_SET)==0);
    char data[5]={0}; CHECK(fread(data,1,4,f)==4 && memcmp(data,"abcZ",4)==0);
    CHECK(fclose(f)==0);
    errno=0; CHECK(fopen(path,"wx")==NULL && errno==EEXIST);
    errno=0; CHECK(fopen(path,"q")==NULL && errno==EINVAL);
    char created[4096]; strcpy(created,templ); strcat(created,".created");
    f=fopen(created,"wx+"); CHECK(f!=NULL);
    phase("created",created);
    CHECK(fputc('C',f)=='C' && fclose(f)==0);
    errno=0; CHECK(fopen(created,"wx")==NULL && errno==EEXIST);
    f=fopen(created,"r"); CHECK(f!=NULL && fgetc(f)=='C' && fclose(f)==0);
    CHECK(unlink(created)==0);
    CHECK(symlink(path,created)==0);
    errno=0; CHECK(open(created,O_RDWR|O_NOFOLLOW)==-1 && errno==ELOOP);
    errno=0; CHECK(fopen(created,"wx")==NULL && errno==EEXIST);
    CHECK(unlink(created)==0);
    strcpy(created,path); strcat(created,"/child");
    errno=0; CHECK(fopen(created,"w")==NULL && errno==ENOTDIR);
    char name[4096]; strcpy(name,templ);
    int tmp=mkstemp(name); CHECK(tmp>=3 && strcmp(name,templ)!=0);
    char second[4096]; strcpy(second,templ);
    int tmp2=mkstemp(second); CHECK(tmp2>=3 && tmp2!=tmp && strcmp(name,second)!=0);
    CHECK(close(tmp2)==0 && unlink(second)==0);
    phase("private-temp",name);
    CHECK(write(tmp,"x",1)==1 && lseek(tmp,0,SEEK_SET)==0);
    FILE *adopted=fdopen(tmp,"r+"); CHECK(adopted != NULL && fileno(adopted)==tmp);
    CHECK(fgetc(adopted)=='x' && fclose(adopted)==0);
    errno=0; CHECK(close(tmp)==-1 && errno==EBADF);
    phase("temp-closed",name);
    CHECK(unlink(name)==0);
    char bad[]="badXXX"; errno=0; CHECK(mkstemp(bad)==-1 && errno==EINVAL && strcmp(bad,"badXXX")==0);
    /* Exclusive creation never truncates a previous C object. */
    f=fopen(path,"r"); CHECK(f!=NULL && fread(data,1,4,f)==4 && memcmp(data,"abcZ",4)==0 && fclose(f)==0);
    printf("stdio-opens passed checks=%d\n",checks); fflush(stdout);
    return 0;
}
int main(int argc,char **argv) {
    if (argc!=4) return 99;
    return strcmp(argv[1],"probe")==0 ? probe(argv[2],argv[3]) : contract(argv[2],argv[3]);
}
