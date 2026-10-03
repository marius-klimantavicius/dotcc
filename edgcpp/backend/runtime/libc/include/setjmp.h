#ifndef __EDG_DOT_LIBC_SETJMP_H
#define __EDG_DOT_LIBC_SETJMP_H
/* Portable translated ABI v2, not a native operating-system jmp_buf. */
#define __EDG_DOT_JMP_BUF_ABI_VERSION 2
typedef struct {
  unsigned long long __activation;
  unsigned int __site;
  unsigned int __version;
  void* __vla_head;
  unsigned long long __vla_identity;
} __edg_jump_buffer;
typedef __edg_jump_buffer jmp_buf[1];
#ifdef __cplusplus
extern "C" {
#endif
int __edg_c_setjmp(void*);
void __edg_c_longjmp(void*,int);
#ifdef __cplusplus
}
#endif
#define setjmp(buffer) __edg_c_setjmp((buffer))
#define longjmp __edg_c_longjmp
#endif
