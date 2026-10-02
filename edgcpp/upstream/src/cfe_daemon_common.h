/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

cfe_daemon_common.h -- Common code for the C++/C front end daemon
                       (cfe_daemon.c) and its corresponding client
                       (cfe_daemon_client.c).

This file is provided as an example of using the front end as a library to
build a multi-threaded client/server C++/C compiler front end.

The client and its corresponding daemon are not (currently) production grade
and are intended for exposition only.
*/

/*
Protocol Documentation

The daemon protocol operates over Unix domain sockets.  The daemon binds and
listens to a two-way socket located at the path computed by
allocating_socket_file_name (see this function for more information about the
exact path of the socket).

The daemon protocol uses a series of bytes (the end_of_message_bytes) below to
delimit the end of messages.  For the purpose of brevity, when this
documentation states that the client or daemon "sends a message containing X"
that means the respective program sends the bytes "X" followed by the
end_of_message_bytes to signal the end of the message.

Clients begin using the daemon by first connecting and then sending a message
containing the daemon level command they would like execute.

1. Ping Command

   The ping daemon command is a very simple command for testing whether the
   socket, and thus daemon, is still alive (and not implemented in
   cfe_daemon_client.c).  This command avoids the added weight and complexity
   of using a front end invocation to test whether the daemon is still alive.
   It is particularly useful for software that queues work to be executed by
   the daemon and needs to keep tabs on whether it's still running (e.g., a
   test driver).

   1. The client immediately upon connecting sends a message containing the
      ping_request_bytes.
   2. The daemon accepts this, sends nothing back, and closes the socket.
   3. The client closes its socket.

2. Exec Command

   This command is used to execute the front end (and is what is implemented by
   cfe_daemon_client.c).

   1. The client immediately upon connecting sends a message containing the
      exec_request_bytes.
   2. If built using DEBUG: the client then sends a message containing the
      translation unit tag (see trans_unit_tag in cfe_daemon.c for more
      information).
   3. The client sends a message containing the number of front end command
      line arguments (i.e., the argc value for EDG_MAIN) as a
      textually-represented integer followed by a null terminator.  This is
      then followed (in the same message) by n null-terminated strings (one per
      command line argument).
   4. The daemon then invokes the front end and sends a message containing the
      textual output of the front end invocation (i.e., what would in most
      conventional configurations be printed to stderr).
   5. The daemon then sends a message containing the textually-represented exit
      code and closes the socket.
   6. The client closes its socket.
*/

#ifndef EDG_DAEMON_COMMON_H
#define EDG_DAEMON_COMMON_H 1

/* C standard library headers used to implement the daemon. */
#include <cstddef>
#include <cstring>

/* Unix headers used to implement the daemon. */
#include <unistd.h>

/* Conditionally open the "edg" namespace. */
BEGIN_EDG_NAMESPACE

constexpr int   rc_daemon_error = 9;

constexpr a_const_char
                *exec_request_bytes = "exec";
                        /* The connection request tag bytes that indicate the
                           requested action is a front end execution. */
constexpr a_const_char
                *ping_request_bytes = "ping";
                        /* The connection request tag bytes that indicate the
                           requested action is a ping (to check to see if the
                           daemon is alive). */
constexpr a_const_char
                *end_of_message_bytes = "7ef77b7a-cd45-4b2e-9498-5e999bb07a06";
                        /* The magic byte sequence used to indicate the end of
                           a message over the socket. */


template<typename a_Func>
inline char* doubling_buffer(a_Func func)
/*
Allocate a character buffer using operator new[] and pass it to the given
function.  If the buffer is sufficiently sized, the function should return
TRUE; otherwise, the function should return FALSE.  The function will be
reinvoked with an increasingly large buffer until it returns TRUE.

Once a sufficiently sized buffer has been created, the buffer will be returned.
The caller is responsible for freeing the buffer.
*/
{
  size_t buf_size = 100;
  char   *buf = NULL;

  /* Note that while Linux will allocate the buffer for you (if you use a NULL
     buffer argument) POSIX does not mandate this behavior. */
  do {
    if (buf != NULL) {
      delete [] buf;
      buf_size *= 2;
    }  /* if */
    buf = new char[buf_size];
  } while (!func(buf, buf_size));
  return buf;
}  /* doubling_buffer */


inline a_const_char* allocating_socket_file_name()
/*
Return a pointer to a null-terminated buffer containing the file name where the
client <-> daemon Unix socket can be found.  The caller is responsible for
freeing the buffer.
*/
{
  a_const_char *daemon_id = getenv("EDG_DAEMON_ID");

  if (daemon_id == NULL) {
    /* Fall back to the default daemon id. */
    daemon_id = "0";
  }  /* if */

  auto apply_file_name = [daemon_id](char *buf, size_t buf_size) -> a_boolean {
    /* For Linux use the directory specified by the Filesystem Hierarchy
       Standard; otherwise, fallback to using /tmp for maximum
       compatibility. */
    int chars_written = snprintf(buf, buf_size,
#if __linux__
                                 "/var/run/user/%lld/cpfe-daemon-%s.sock",
#else /* !__linux__ */
                                 "/tmp/cpfe-daemon-%lld-%s.sock",
#endif /* __linux__ */
                                 (long long)geteuid(), daemon_id);
    return chars_written >= 0 && ((size_t)chars_written) <= buf_size;
  };
  return doubling_buffer(apply_file_name);
}  /* allocating_socket_file_name */


/*
A type that encapsulates a buffered read from a Unix socket.
*/
struct a_socket_reader {
  a_socket_reader(int socket_fd_val)
    : socket_fd(socket_fd_val)
    {}
  template<typename a_Func>
  a_boolean read_message_parts(a_Func callback_fn);
private:
  template<typename a_Func>
  a_boolean forward_buffer_from(int offset, a_Func callback_fn);
  static constexpr size_t
                buffer_size = 1000;
                        /* The size of the socket reader buffer. */
  int           socket_fd;
                        /* The socket file descriptor to read from. */
  char          buffer[buffer_size] = {};
                        /* The buffer used to read from the file descriptor. */
  ssize_t       num_buffer_chars = 0;
                        /* The number of characters written to the buffer
                           during the last read operation. */
  int           unprocessed_bytes_start = -1;
                        /* If there are no bytes from the previous read call,
                           this is -1.  If there are bytes from the previous
                           read call this is the offset from the start of
                           buffer to those bytes. */
  int           eom_matches = 0;
                        /* The number of end of message characters that have
                           been matched. */
};  /* a_socket_reader */


template<typename a_Func>
a_boolean a_socket_reader::read_message_parts(a_Func callback_fn)
/*
Given a callback (accepting a_const_char* and size_t) read the next message
from the socket calling the given callback with any new message parts.  If the
complete message has been read without any errors, return TRUE; otherwise,
return FALSE.
*/
{
  a_boolean result = FALSE;

  if (this->unprocessed_bytes_start != -1) {
    /* Reset the unprocessed byte start and then process the remaining bytes
       from the previous call to read_message_parts. */
    int offset = this->unprocessed_bytes_start;

    this->unprocessed_bytes_start = -1;
    if (this->forward_buffer_from(offset, callback_fn)) {
      /* The previous buffer contained an additional complete message. */
      goto complete_message;
    }  /* if */
  }  /* if */
  while (TRUE) {
    this->num_buffer_chars = read(this->socket_fd, this->buffer, buffer_size);
    if (this->num_buffer_chars <= 0) {
      break;
    }  /* if */
    if (forward_buffer_from(0, callback_fn)) {
      goto complete_message;
    }  /* if */
  }  /* while */
  goto done;
complete_message:
  result = TRUE;
done:
  return result;
}  /* a_socket_reader::read_message_parts */


template<typename a_Func>
a_boolean a_socket_reader::forward_buffer_from(int offset, a_Func callback_fn)
/*
Forward buffer contents from the given starting position to the callback (see
read_message_parts for more information about the callback).  If a complete
message was found, return TRUE; otherwise, return FALSE.
*/
{
  a_boolean result = FALSE;
  int       num_matches_needed = strlen(end_of_message_bytes);
  ssize_t   end_of_message_part = this->num_buffer_chars;

  for (int i = offset; i < this->num_buffer_chars; ++i) {
    if (this->buffer[i] == end_of_message_bytes[this->eom_matches]) {
      if (++(this->eom_matches) == num_matches_needed) {
        /* This is the end of the message. */
        int next_idx = i + 1;

        if (next_idx < this->num_buffer_chars) {
          /* There are unprocessed bytes still in the buffer after the end of
             message string starting at the next position. */
          this->unprocessed_bytes_start = next_idx;
        }  /* if */
        /* Update the end of message part to reflect the fact no characters
           after this point should be included in this message. */
        end_of_message_part = next_idx;
        break;
      }  /* if */
    } else if (eom_matches > 0) {
      /* This is not the end of message string. */
      if (eom_matches > i) {
        /* This partial match starts before the current buffer: replay the
           matching characters and update the initial offset so that bytes
           aren't repeated. */
        callback_fn(end_of_message_bytes, (size_t)eom_matches);
        offset = i;
      }  /* if */
      this->eom_matches = 0;
    }  /* if */
  }  /* for */

  /* Forward all characters in the buffer that have not yet been forwarded
     and are known to not be part of the end of message bytes. */
  ssize_t num_chars_to_forward = (end_of_message_part -
                                                 (offset + this->eom_matches));
  if (num_chars_to_forward > 0) {
    callback_fn(this->buffer + offset, (size_t)num_chars_to_forward);
  }  /* if */
  if (this->eom_matches == num_matches_needed) {
    result = TRUE;
    this->eom_matches = 0;
  }  /* if */
  return result;
}  /* a_socket_reader::forward_buffer_from */

/* Conditionally close the "edg" namespace. */
END_EDG_NAMESPACE

#endif /* EDG_DAEMON_COMMON_H */

