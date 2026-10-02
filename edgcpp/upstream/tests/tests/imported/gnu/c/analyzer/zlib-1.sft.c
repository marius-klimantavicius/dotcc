//type: fp
//options: 
# 0 "./analyzer/zlib-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/zlib-1.c"
# 1 "./analyzer/analyzer-decls.h" 1







extern void __analyzer_break (void);




extern void __analyzer_describe (int verbosity, ...);


extern void __analyzer_dump (void);


extern void __analyzer_dump_capacity (const void *ptr);


extern void __analyzer_dump_escaped (void);
# 32 "./analyzer/analyzer-decls.h"
extern void __analyzer_dump_exploded_nodes (int);


extern void __analyzer_dump_named_constant (const char *name);



extern void __analyzer_dump_path (void);


extern void __analyzer_dump_region_model (void);




extern void __analyzer_dump_state (const char *name, ...);



extern void __analyzer_eval (int);


extern void *__analyzer_get_unknown_ptr (void);
# 2 "./analyzer/zlib-1.c" 2

typedef void (*free_func)(void *opaque, void *address);

typedef struct z_stream_s {
  struct internal_state *state;
  free_func zfree;
  void *opaque;
} z_stream;

struct internal_state {
  z_stream *strm;
  int status;
  unsigned char *pending_buf;
  unsigned char *window;
  unsigned short *prev;
  unsigned short *head;
};

int deflateEnd(z_stream *strm)
{
  int status;

  __analyzer_dump_exploded_nodes (0);

  if (strm == 0 || strm->state == 0)
    return (-2);

  __analyzer_dump_exploded_nodes (0);

  status = strm->state->status;
  if (status != 42 && status != 113 && status != 666) {
    return (-2);
  }

  __analyzer_dump_exploded_nodes (0);

  if (strm->state->pending_buf)
    (*(strm->zfree))(strm->opaque, (void *)(strm->state->pending_buf));

  __analyzer_dump_exploded_nodes (0);

  if (strm->state->head)
      (*(strm->zfree))(strm->opaque, (void *)(strm->state->head));

  __analyzer_dump_exploded_nodes (0);

  if (strm->state->prev)
    (*(strm->zfree))(strm->opaque, (void *)(strm->state->prev));

  __analyzer_dump_exploded_nodes (0);

  if (strm->state->window)
    (*(strm->zfree))(strm->opaque, (void *)(strm->state->window));

  __analyzer_dump_exploded_nodes (0);

  (*(strm->zfree))(strm->opaque, (void *)(strm->state));
  strm->state = 0;

  __analyzer_dump_exploded_nodes (0);

  return status == 113 ? (-3) : 0;
}
