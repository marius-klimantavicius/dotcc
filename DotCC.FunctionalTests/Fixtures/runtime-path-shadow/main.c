#define _XOPEN_SOURCE 700
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <dirent.h>

int Path(int value){return value+1;}
int probe(void){
  FILE *file=tmpfile();if(!file)return 1;
  if(fwrite("path",1,4,file)!=4 || fseek(file,0,SEEK_SET))return 2;
  char bytes[5]={0};if(fread(bytes,1,4,file)!=4 || strcmp(bytes,"path") || fclose(file))return 3;
  char *full=realpath(".",0);if(!full || !*full)return 4;free(full);
  DIR *directory=(DIR*)opendir(".");if(!directory)return 5;
  int dots=0;struct dirent *entry;
  while((entry=(struct dirent*)readdir(directory))){if(!strcmp(entry->d_name,"."))dots|=1;if(!strcmp(entry->d_name,".."))dots|=2;}
  if(closedir(directory) || dots!=3)return 6;
  return Path(41)==42?0:7;
}
int main(void){int result=probe();if(result)return result;puts("Path identifier and runtime file operations: PASS");return 0;}
