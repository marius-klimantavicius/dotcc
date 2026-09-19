#include <stddef.h>
#include <stdio.h>
#include <sys/socket.h>
#include <sys/uio.h>
#define O(t,f) printf(#t "." #f "=%zu\n",offsetof(struct t,f))
int main(void){
 printf("msghdr=%zu/%zu\n",sizeof(struct msghdr),_Alignof(struct msghdr));
 O(msghdr,msg_name);O(msghdr,msg_namelen);O(msghdr,msg_iov);O(msghdr,msg_iovlen);O(msghdr,msg_control);O(msghdr,msg_controllen);O(msghdr,msg_flags);
 printf("iovec=%zu/%zu\n",sizeof(struct iovec),_Alignof(struct iovec));O(iovec,iov_base);O(iovec,iov_len);
 return 0;
}
