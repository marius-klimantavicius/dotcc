/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

cfe_daemon_client.c -- Unix socket based client for the C++/C front end daemon.

This file is provided as an example of using the front end as a library to
build a multi-threaded client/server C++/C compiler front end.

This client and its corresponding daemon are not (currently) production grade
and are intended for exposition only.

See cfe_daemon_common.h for documentation on the daemon protocol.
*/

#include "basics.h"

/* Common daemon code. */
#include "cfe_daemon_common.h"

/* C standard library headers used to implement the daemon client. */
#include <cstdlib>
#include <cstdio>

/* Unix headers used to implement the daemon client. */
#include <sys/socket.h>
#include <sys/un.h>

/*
Note that the EDG namespace is not opened here.  Instead we do a
using-directive if the EDG code is in the edg namespace.
*/
USING_NAMESPACE_EDG

static a_const_char* allocating_cwd()
/*
Return a pointer to a null-terminated buffer containing the current working
directory.  The caller is responsible for freeing the buffer.
*/
{
  auto apply_cwd = [](char *buf, size_t buf_size) -> a_boolean {
    return getcwd(buf, buf_size) != NULL;
  };
  return doubling_buffer(apply_cwd);
}  /* allocating_cwd */


constexpr a_const_char
                *error_prefix = "C++/C DAEMON CLIENT ERROR: ";
                        /* The common string prepended to error messages. */


static void error_str(a_const_char *str)
/*
Emit a C++/C client error with the given error message string.
*/
{
  fprintf(stderr, "%s%s\n", error_prefix, str);
}  /* error_str */


static void print_message(const char *msg, size_t msg_len)
/*
This function performs an optimized print of the given message of the given
length (possibly containing internal null characters).

This function takes advantage of the fact that fprintf of a string of a given
length is significantly faster than individual putc calls to minimize the
number of system calls and maximize printing performance.
*/
{
  size_t str_len = 0;

  /* Calculate the string length, terminating if a null character is
     encountered before the full string length.  Note that the message is not
     guaranteed to be null-terminated so strlen cannot be used here. */
  for (size_t i = 0; i != msg_len; ++i) {
    if (msg[i] == '\0') {
      break;
    }  /* if */
    ++str_len;
  }  /* for */
  if (str_len == msg_len) {
    /* The message contained no null characters; this is the optimal (and
       common) case. */
    fprintf(stderr, "%.*s", (int)msg_len, msg);
  } else {
    /* Internal null characters need to be printed.  The optimal fprintf call
       will short circuit on the null character before it reaches msg_len
       characters.  Thus, we print what fprintf can print, the null
       character(s), and then recurse if there's any remaining content. */
    fprintf(stderr, "%.*s", (int)str_len, msg);

    size_t num_nulls = 0;
    for (; str_len + num_nulls != msg_len; ++num_nulls) {
      char curr_char = msg[str_len + num_nulls];

      if (curr_char != '\0') {
        break;
      }  /* if */
      fputc(curr_char, stderr);
    }  /* for */
    size_t chars_printed = str_len + num_nulls;
    if (chars_printed < msg_len) {
      /* There are characters that still remain: recurse to finish printing the
         message. */
      print_message(msg + chars_printed, msg_len - chars_printed);
    }  /* if */
  }  /* if */
}  /* print_message */


int main(int argc, char *argv[])
/*
The main routine for the front end Unix socket client.
*/
{
  int          return_value = rc_daemon_error;
  a_const_char *socket_address = allocating_socket_file_name();
  int          client_socket_fd = -1;

  {
    client_socket_fd = socket(AF_UNIX, SOCK_STREAM, 0);
    if (client_socket_fd == -1) {
      goto socket_set_up_error;
    }  /* if */

    /* Construct the client address structure. */
    size_t      socket_path_bytes = strlen(socket_address) + 1;
    sockaddr_un client_socket_addr = {};
    if (socket_path_bytes > sizeof(client_socket_addr.sun_path)) {
      goto socket_path_too_long_error;
    }  /* if */
    client_socket_addr.sun_family = AF_UNIX;
    (void)memcpy(client_socket_addr.sun_path, socket_address,
                 socket_path_bytes);

    int connect_result = connect(client_socket_fd,
                                 (const sockaddr*)&client_socket_addr,
                                 sizeof(client_socket_addr));
    if (connect_result == -1) {
      goto connection_set_up_error;
    }  /* if */
    /* Inform the daemon this is an execution request. */
    write(client_socket_fd, exec_request_bytes, strlen(exec_request_bytes));
    write(client_socket_fd, end_of_message_bytes,
          strlen(end_of_message_bytes));

#if DEBUG
    /* In DEBUG builds capture the value of EDG_TRANSLATION_UNIT_TAG, which
       will be set to the thread-local trans_unit_tag in the daemon. */
    a_const_char *tu_tag = getenv("EDG_TRANSLATION_UNIT_TAG");
    if (tu_tag == NULL) {
      /* Fall back to an empty string. */
      tu_tag = "";
    }  /* if */
    write(client_socket_fd, tu_tag, strlen(tu_tag));
    write(client_socket_fd, end_of_message_bytes,
          strlen(end_of_message_bytes));
#endif /* DEBUG */

    /* Write the number of arguments (+1 to account for the --wdir argument
       added by this function). */
    constexpr unsigned buffer_size = 100;
    char buffer[buffer_size];
    int  bytes_written = snprintf(buffer, buffer_size, "%d", argc + 1);
    write(client_socket_fd, buffer, (size_t)(bytes_written + 1));

    /* Write argv[0] to pass along the process name in the correct position. */
    write(client_socket_fd, argv[0], strlen(argv[0]) + 1);

    /* Write the implicit --wdir value to communicate the location where this
       process was invoked. */
    constexpr const char *wdir_flag_name = "--wdir=";
    a_const_char *wdir = allocating_cwd();
    write(client_socket_fd, wdir_flag_name, strlen(wdir_flag_name));
    write(client_socket_fd, wdir, strlen(wdir) + 1);
    delete [] wdir;
    /* Write the arguments given to this process. */
    for (int i = 1; i < argc; ++i) {
      write(client_socket_fd, argv[i], strlen(argv[i]) + 1);
    }  /* for */
    write(client_socket_fd, end_of_message_bytes,
          strlen(end_of_message_bytes));

    /* Read the output. */
    a_socket_reader reader(client_socket_fd);
    if (!reader.read_message_parts(print_message)) {
      goto unexpected_hang_up;
    }  /* if */

    /* Read the exit code. */
    constexpr unsigned max_exit_code_len = 3;
    char     exit_code_buffer[max_exit_code_len + 1] = {};
    unsigned exit_code_len = 0;
    auto     update_exit_code = [&exit_code_buffer, &exit_code_len]
                                            (const char *msg, size_t msg_len) {
      for (size_t i = 0; i < msg_len; ++i) {
        if (exit_code_len < max_exit_code_len) {
          exit_code_buffer[exit_code_len] = msg[i];
        }  /* if */
        ++exit_code_len;
      }  /* for */
    };
    if (!reader.read_message_parts(update_exit_code)) {
      goto unexpected_hang_up;
    }  /* if */
    if (exit_code_len > max_exit_code_len) {
      goto exit_code_overflow;
    }  /* if */
    return_value = atoi(exit_code_buffer);
  }
  goto done;
socket_set_up_error:
  error_str("Unix socket set up failed.");
  goto done;
socket_path_too_long_error:
  fprintf(stderr, "%sUnix socket path (\"%s\") is too long.\n", error_prefix,
          socket_address);
  return_value = 1;
  goto done;
connection_set_up_error:
  error_str("Unix socket connection failed; is the daemon running?");
  goto done;
unexpected_hang_up:
  error_str("Unexpected socket shutdown.");
  goto done;
exit_code_overflow:
  error_str("Exit code buffer overflow.");
  goto done;
done:
  if (client_socket_fd != -1) {
    close(client_socket_fd);
  }  /* if */
  delete [] socket_address;
  return return_value;
}  /* main */


