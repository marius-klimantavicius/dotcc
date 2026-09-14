#define _GNU_SOURCE 1
#include <errno.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdint.h>
#include <fcntl.h>
#include <unistd.h>
#include <sys/stat.h>
#ifdef BLINK_MANAGED_PATHS
#include "host-io.h"
#include "host-paths.h"
#define ROOT ""
#define ENAMETOOLONG 36
#else
#if ENAMETOOLONG != 36
#error native pathname error constant differs from profile
#endif
#ifndef ROOT
#error native fixture root is required
#endif
#endif
#define CHECK(x) do { if(!(x)){printf("path failure %d\n",__LINE__);return __LINE__;} } while(0)
int PathProbe(void) {
  char cwd[4096],saved[4096],resolved[4096],small[2]={'X','Y'};
  char *owned;
  struct stat info;
  int directory,duplicate,file;
  CHECK(!chdir(ROOT "/a"));
  CHECK(getcwd(cwd,sizeof(cwd))==cwd && !strcmp(cwd,ROOT "/a"));
  strcpy(saved,cwd);
  CHECK(!getcwd(small,sizeof(small)) && errno==ERANGE && small[0]=='X' && small[1]=='Y');
  CHECK(!getcwd(small,0) && errno==EINVAL);
  CHECK(!getcwd(0,1) && errno==ERANGE);
  owned=getcwd(0,0);CHECK(owned && !strcmp(owned,saved));free(owned);
  owned=getcwd(0,4096);CHECK(owned && !strcmp(owned,saved));free(owned);
  CHECK(!stat("seed",&info) && info.st_size==4);
  file=open("seed",O_RDONLY);CHECK(file>=0);
  CHECK(fchdir(file)==-1 && errno==ENOTDIR);CHECK(!close(file));
  CHECK(chdir("seed")==-1 && errno==ENOTDIR);
  CHECK(chdir("missing")==-1 && errno==ENOENT);
  CHECK(chdir("")==-1 && errno==ENOENT);
  CHECK(chdir("seed/../b")==-1 && errno==ENOTDIR);
  CHECK(getcwd(cwd,sizeof(cwd))==cwd && !strcmp(cwd,saved));
  CHECK(realpath("./b/../seed",resolved)==resolved && !strcmp(resolved,ROOT "/a/seed"));
  CHECK(!realpath("seed/child",resolved) && errno==ENOTDIR);
  CHECK(!realpath("missing",resolved) && errno==ENOENT);
  owned=realpath("seed",0);CHECK(owned && !strcmp(owned,ROOT "/a/seed"));free(owned);
  directory=open("b",O_RDONLY|O_DIRECTORY);CHECK(directory>=0);
  duplicate=dup(directory);CHECK(duplicate>=0 && !close(directory));
  CHECK(fchdir(directory)==-1 && errno==EBADF);
  CHECK(!fchdir(duplicate));CHECK(!close(duplicate));
  CHECK(getcwd(cwd,sizeof(cwd))==cwd && !strcmp(cwd,ROOT "/a/b"));
  CHECK(!fstatat(AT_FDCWD,"../seed",&info,0) && info.st_size==4);
  file=openat(AT_FDCWD,"../seed",O_RDONLY);CHECK(file>=0 && !close(file));
  CHECK(!chdir(".."));CHECK(!chdir("../other"));
  CHECK(getcwd(cwd,sizeof(cwd))==cwd && !strcmp(cwd,ROOT "/other"));
  CHECK(!chdir(ROOT "/\xc3\xa9"));
  CHECK(realpath("leaf",resolved)==resolved && !strcmp(resolved,ROOT "/\xc3\xa9/leaf"));
  CHECK(!chdir(ROOT "/a"));
  puts("private cwd, descriptor-relative lookup, canonical paths and owned strings: PASS");
  return 0;
}
#ifdef BLINK_MANAGED_PATHS
int PathPrivate(void) {
  char cwd[4096],out[4096],bad[2]={(char)0xc0,0},longpath[4097];
  memset(out,'Z',sizeof(out));memset(longpath,'a',4096);longpath[4096]=0;
  CHECK(!getcwd(0,4098) && errno==ENOMEM);
  CHECK(!realpath("missing",out) && errno==ENOENT && out[0]=='Z');
  CHECK(!realpath(0,out) && errno==EFAULT && out[0]=='Z');
  CHECK(chdir(0)==-1 && errno==EFAULT);
  CHECK(chdir(bad)==-1 && errno==EINVAL);
  CHECK(!realpath(bad,out) && errno==EINVAL && out[0]=='Z');
  CHECK(chdir(longpath)==-1 && errno==ENAMETOOLONG);
  CHECK(!realpath(longpath,out) && errno==ENAMETOOLONG);
  CHECK(!chdir("/"));CHECK(chdir("..")==-1 && errno==EACCES);
  CHECK(!realpath("../a",out) && errno==EACCES);
  CHECK(getcwd(cwd,sizeof(cwd))==cwd && !strcmp(cwd,"/"));
  CHECK(fchdir(1)==-1 && errno==ENOTDIR);
  CHECK(fchdir(-1)==-1 && errno==EBADF);
  CHECK(!chdir("/a"));errno=123;CHECK(getcwd(cwd,sizeof(cwd))==cwd && errno==123);
  return 0;
}
struct SavedPaths { char *cwd; char *file; };
void *SavePaths(void) {
  struct SavedPaths *saved=malloc(sizeof(*saved));
  if(!saved)return 0;
  saved->cwd=getcwd(0,0);saved->file=realpath("seed",0);
  if(!saved->cwd || !saved->file){free(saved->cwd);free(saved->file);free(saved);return 0;}
  return saved;
}
int CheckSavedPaths(void *memory) {
  struct SavedPaths *saved=memory;
  int ok=!strcmp(saved->cwd,"/a") && !strcmp(saved->file,"/a/seed");
  free(saved->cwd);free(saved->file);free(saved);
  return ok ? 0 : 1;
}
int PathWorker(int second) {
  char cwd[4096];struct stat info;
  CHECK(getcwd(cwd,sizeof(cwd))==cwd && !strcmp(cwd,second ? "/other" : "/a"));
  CHECK(!stat("seed",&info) && info.st_size==(second ? 7 : 4));
  return 0;
}
int PathUnavailable(int error) {
  char out[20];memset(out,'Z',sizeof(out));
  CHECK(!getcwd(out,sizeof(out)) && errno==error && out[0]=='Z');
  CHECK(chdir("/")==-1 && errno==error);
  CHECK(fchdir(1)==-1 && errno==error);
  CHECK(!realpath("/",out) && errno==error && out[0]=='Z');
  return 0;
}
#else
int main(void){return PathProbe();}
#endif
