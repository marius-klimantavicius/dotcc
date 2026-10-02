//type: fp
//options: --c17 --strict_gnu -w
// { dg-do compile }
// { dg-options "-std=gnu17 -O1 -w" }

typedef void *SCM;
void set_socket_io_ports();
void STk_socket_accept(SCM line_buffered) {
  if (!line_buffered)
    line_buffered = (SCM)3;
  set_socket_io_ports(line_buffered != 1);
}
