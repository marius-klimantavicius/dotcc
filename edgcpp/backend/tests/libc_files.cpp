#include <stdio.h>
#include <string.h>
#include <errno.h>

extern "C" int run_file_case(int which,const char *path) {
  char bytes[64],buffer[64];
  memset(bytes,0,sizeof bytes);memset(buffer,0,sizeof buffer);
  FILE *file=fopen(path,"w+b");if(!file)return 0;
  int good=0;
  switch(which) {
    case 0: {
      bytes[0]=0;bytes[1]=(char)128;bytes[2]=(char)255;bytes[3]='\n';
      size_t (*write_fn)(const void*,size_t,size_t,FILE*)=fwrite;
      good=write_fn(bytes,2,2,file)==2 && ftell(file)==4 && fflush(file)==0 && fseek(file,0,SEEK_SET)==0;
      good=good && fread(buffer,1,4,file)==4 && memcmp(bytes,buffer,4)==0 && !feof(file);
      good=good && fgetc(file)==EOF && feof(file) && !ferror(file);break;
    }
    case 1: {
      good=setvbuf(file,buffer,_IOFBF,sizeof buffer)==0 && fputs("abc",file)>=0 && ftell(file)==3;
      good=good && fflush(file)==0 && fseek(file,0,SEEK_SET)==0 && getc(file)=='a';break;
    }
    case 2: {
      good=fputs("ABC",file)>=0 && fseek(file,0,SEEK_SET)==0 && fread(bytes,2,2,file)==1;
      good=good && bytes[0]=='A' && bytes[1]=='B' && bytes[2]=='C' && feof(file);
      good=good && ungetc(254,file)==254 && !feof(file) && ftell(file)==2 && fgetc(file)==254 && ftell(file)==3;break;
    }
    case 3: {
      good=fputs("ABC",file)>=0 && fseek(file,1,SEEK_SET)==0 && getc(file)=='B' && ungetc('x',file)=='x';
      good=good && fseek(file,1,SEEK_CUR)==0 && ftell(file)==2 && getc(file)=='C';break;
    }
    case 4: {
      good=setvbuf(file,0,_IONBF,0)==0 && fputc(511,file)==255 && putc('a',file)=='a';
      good=good && fseek(file,-1,SEEK_END)==0 && getc(file)=='a';break;
    }
    case 5: {
      good=fputs("abc",file)>=0 && fseek(file,0,SEEK_SET)==0 && fread(bytes,1,9,file)==3 && feof(file);
      errno=71;clearerr(file);good=good && !feof(file) && !ferror(file) && errno==71;break;
    }
    case 6: {
      good=fwrite(bytes,0,5,file)==0 && fread(bytes,1,0,file)==0 && ftell(file)==0 && !feof(file) && !ferror(file);break;
    }
    case 7: {
      good=fputs("AB",file)>=0 && fclose(file)==0;file=fopen(path,"a+b");if(!file)return 0;
      good=good && fseek(file,0,SEEK_SET)==0 && fputc('C',file)=='C' && fflush(file)==0;
      good=good && fseek(file,0,SEEK_SET)==0 && fread(bytes,1,3,file)==3 && memcmp(bytes,"ABC",3)==0;break;
    }
    case 8: {
      good=fputs("abc\ndef",file)>=0 && fseek(file,0,SEEK_SET)==0;
      good=good && fgets(bytes,3,file)==bytes && strcmp(bytes,"ab")==0 && ftell(file)==2;
      good=good && fgets(bytes,64,file)==bytes && strcmp(bytes,"c\n")==0 && ftell(file)==4;
      good=good && fgets(bytes,1,file)==bytes && bytes[0]==0 && ftell(file)==4;
      good=good && fgets(bytes,64,file)==bytes && strcmp(bytes,"def")==0 && feof(file);
      good=good && fgets(bytes,64,file)==0;break;
    }
    case 9: {
      good=fputs("AB",file)>=0 && fclose(file)==0;file=fopen(path,"a+b");if(!file)return 0;
      good=good && ftell(file)==0 && fgetc(file)=='A';break;
    }
    case 10: {
      good=fputs("renamed",file)>=0 && fclose(file)==0;
      char other[1024];if(strlen(path)+9>=sizeof other)return 0;
      strcpy(other,path);strcat(other,".renamed");
      good=good && rename(path,other)==0;file=fopen(other,"rb");if(!file)return 0;
      good=good && fgets(bytes,64,file)==bytes && strcmp(bytes,"renamed")==0;
      good=good && fclose(file)==0 && remove(other)==0;
      errno=0;good=good && remove(other)==-1 && errno==2;return good;
    }
  }
  return fclose(file)==0 && good;
}
extern "C" FILE **stdio_global_address() {return &stdout;}
extern "C" int stdio_globals_distinct() {return stdin && stdout && stderr && stdin!=stdout && stdout!=stderr;}
extern "C" int file_positions(const char *path) {
  FILE *file=fopen(path,"w+b");if(!file)return 0;
  fpos_t start={0};int good=fgetpos(file,&start)==0 && fputs("abc",file)>=0;
  good=good && fsetpos(file,&start)==0 && fgetc(file)=='a';
  return fclose(file)==0 && good;
}
