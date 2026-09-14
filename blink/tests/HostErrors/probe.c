#define _GNU_SOURCE 1
#include <errno.h>
#include <stdio.h>
#ifdef BLINK_MANAGED_ERRORS
#include "host-errors.h"
#endif
#ifndef E2BIG
#error missing host error macro: E2BIG
#endif
#ifndef EACCES
#error missing host error macro: EACCES
#endif
#ifndef EADDRINUSE
#error missing host error macro: EADDRINUSE
#endif
#ifndef EADDRNOTAVAIL
#error missing host error macro: EADDRNOTAVAIL
#endif
#ifndef EADV
#error missing host error macro: EADV
#endif
#ifndef EAFNOSUPPORT
#error missing host error macro: EAFNOSUPPORT
#endif
#ifndef EAGAIN
#error missing host error macro: EAGAIN
#endif
#ifndef EALREADY
#error missing host error macro: EALREADY
#endif
#ifndef EBADE
#error missing host error macro: EBADE
#endif
#ifndef EBADF
#error missing host error macro: EBADF
#endif
#ifndef EBADFD
#error missing host error macro: EBADFD
#endif
#ifndef EBADMSG
#error missing host error macro: EBADMSG
#endif
#ifndef EBADR
#error missing host error macro: EBADR
#endif
#ifndef EBADRQC
#error missing host error macro: EBADRQC
#endif
#ifndef EBADSLT
#error missing host error macro: EBADSLT
#endif
#ifndef EBFONT
#error missing host error macro: EBFONT
#endif
#ifndef EBUSY
#error missing host error macro: EBUSY
#endif
#ifndef ECANCELED
#error missing host error macro: ECANCELED
#endif
#ifndef ECHILD
#error missing host error macro: ECHILD
#endif
#ifndef ECHRNG
#error missing host error macro: ECHRNG
#endif
#ifndef ECOMM
#error missing host error macro: ECOMM
#endif
#ifndef ECONNABORTED
#error missing host error macro: ECONNABORTED
#endif
#ifndef ECONNREFUSED
#error missing host error macro: ECONNREFUSED
#endif
#ifndef ECONNRESET
#error missing host error macro: ECONNRESET
#endif
#ifndef EDEADLK
#error missing host error macro: EDEADLK
#endif
#ifndef EDEADLOCK
#error missing host error macro: EDEADLOCK
#endif
#ifndef EDESTADDRREQ
#error missing host error macro: EDESTADDRREQ
#endif
#ifndef EDOM
#error missing host error macro: EDOM
#endif
#ifndef EDOTDOT
#error missing host error macro: EDOTDOT
#endif
#ifndef EDQUOT
#error missing host error macro: EDQUOT
#endif
#ifndef EEXIST
#error missing host error macro: EEXIST
#endif
#ifndef EFAULT
#error missing host error macro: EFAULT
#endif
#ifndef EFBIG
#error missing host error macro: EFBIG
#endif
#ifndef EHOSTDOWN
#error missing host error macro: EHOSTDOWN
#endif
#ifndef EHOSTUNREACH
#error missing host error macro: EHOSTUNREACH
#endif
#ifndef EHWPOISON
#error missing host error macro: EHWPOISON
#endif
#ifndef EIDRM
#error missing host error macro: EIDRM
#endif
#ifndef EILSEQ
#error missing host error macro: EILSEQ
#endif
#ifndef EINPROGRESS
#error missing host error macro: EINPROGRESS
#endif
#ifndef EINTR
#error missing host error macro: EINTR
#endif
#ifndef EINVAL
#error missing host error macro: EINVAL
#endif
#ifndef EIO
#error missing host error macro: EIO
#endif
#ifndef EISCONN
#error missing host error macro: EISCONN
#endif
#ifndef EISDIR
#error missing host error macro: EISDIR
#endif
#ifndef EISNAM
#error missing host error macro: EISNAM
#endif
#ifndef EKEYEXPIRED
#error missing host error macro: EKEYEXPIRED
#endif
#ifndef EKEYREJECTED
#error missing host error macro: EKEYREJECTED
#endif
#ifndef EKEYREVOKED
#error missing host error macro: EKEYREVOKED
#endif
#ifndef EL2HLT
#error missing host error macro: EL2HLT
#endif
#ifndef EL2NSYNC
#error missing host error macro: EL2NSYNC
#endif
#ifndef EL3HLT
#error missing host error macro: EL3HLT
#endif
#ifndef EL3RST
#error missing host error macro: EL3RST
#endif
#ifndef ELIBACC
#error missing host error macro: ELIBACC
#endif
#ifndef ELIBBAD
#error missing host error macro: ELIBBAD
#endif
#ifndef ELIBEXEC
#error missing host error macro: ELIBEXEC
#endif
#ifndef ELIBMAX
#error missing host error macro: ELIBMAX
#endif
#ifndef ELIBSCN
#error missing host error macro: ELIBSCN
#endif
#ifndef ELNRNG
#error missing host error macro: ELNRNG
#endif
#ifndef ELOOP
#error missing host error macro: ELOOP
#endif
#ifndef EMEDIUMTYPE
#error missing host error macro: EMEDIUMTYPE
#endif
#ifndef EMFILE
#error missing host error macro: EMFILE
#endif
#ifndef EMLINK
#error missing host error macro: EMLINK
#endif
#ifndef EMSGSIZE
#error missing host error macro: EMSGSIZE
#endif
#ifndef EMULTIHOP
#error missing host error macro: EMULTIHOP
#endif
#ifndef ENAMETOOLONG
#error missing host error macro: ENAMETOOLONG
#endif
#ifndef ENAVAIL
#error missing host error macro: ENAVAIL
#endif
#ifndef ENETDOWN
#error missing host error macro: ENETDOWN
#endif
#ifndef ENETRESET
#error missing host error macro: ENETRESET
#endif
#ifndef ENETUNREACH
#error missing host error macro: ENETUNREACH
#endif
#ifndef ENFILE
#error missing host error macro: ENFILE
#endif
#ifndef ENOANO
#error missing host error macro: ENOANO
#endif
#ifndef ENOBUFS
#error missing host error macro: ENOBUFS
#endif
#ifndef ENOCSI
#error missing host error macro: ENOCSI
#endif
#ifndef ENODATA
#error missing host error macro: ENODATA
#endif
#ifndef ENODEV
#error missing host error macro: ENODEV
#endif
#ifndef ENOENT
#error missing host error macro: ENOENT
#endif
#ifndef ENOEXEC
#error missing host error macro: ENOEXEC
#endif
#ifndef ENOKEY
#error missing host error macro: ENOKEY
#endif
#ifndef ENOLCK
#error missing host error macro: ENOLCK
#endif
#ifndef ENOLINK
#error missing host error macro: ENOLINK
#endif
#ifndef ENOMEDIUM
#error missing host error macro: ENOMEDIUM
#endif
#ifndef ENOMEM
#error missing host error macro: ENOMEM
#endif
#ifndef ENOMSG
#error missing host error macro: ENOMSG
#endif
#ifndef ENONET
#error missing host error macro: ENONET
#endif
#ifndef ENOPKG
#error missing host error macro: ENOPKG
#endif
#ifndef ENOPROTOOPT
#error missing host error macro: ENOPROTOOPT
#endif
#ifndef ENOSPC
#error missing host error macro: ENOSPC
#endif
#ifndef ENOSR
#error missing host error macro: ENOSR
#endif
#ifndef ENOSTR
#error missing host error macro: ENOSTR
#endif
#ifndef ENOSYS
#error missing host error macro: ENOSYS
#endif
#ifndef ENOTBLK
#error missing host error macro: ENOTBLK
#endif
#ifndef ENOTCONN
#error missing host error macro: ENOTCONN
#endif
#ifndef ENOTDIR
#error missing host error macro: ENOTDIR
#endif
#ifndef ENOTEMPTY
#error missing host error macro: ENOTEMPTY
#endif
#ifndef ENOTNAM
#error missing host error macro: ENOTNAM
#endif
#ifndef ENOTRECOVERABLE
#error missing host error macro: ENOTRECOVERABLE
#endif
#ifndef ENOTSOCK
#error missing host error macro: ENOTSOCK
#endif
#ifndef ENOTSUP
#error missing host error macro: ENOTSUP
#endif
#ifndef ENOTTY
#error missing host error macro: ENOTTY
#endif
#ifndef ENOTUNIQ
#error missing host error macro: ENOTUNIQ
#endif
#ifndef ENXIO
#error missing host error macro: ENXIO
#endif
#ifndef EOPNOTSUPP
#error missing host error macro: EOPNOTSUPP
#endif
#ifndef EOVERFLOW
#error missing host error macro: EOVERFLOW
#endif
#ifndef EOWNERDEAD
#error missing host error macro: EOWNERDEAD
#endif
#ifndef EPERM
#error missing host error macro: EPERM
#endif
#ifndef EPFNOSUPPORT
#error missing host error macro: EPFNOSUPPORT
#endif
#ifndef EPIPE
#error missing host error macro: EPIPE
#endif
#ifndef EPROTO
#error missing host error macro: EPROTO
#endif
#ifndef EPROTONOSUPPORT
#error missing host error macro: EPROTONOSUPPORT
#endif
#ifndef EPROTOTYPE
#error missing host error macro: EPROTOTYPE
#endif
#ifndef ERANGE
#error missing host error macro: ERANGE
#endif
#ifndef EREMCHG
#error missing host error macro: EREMCHG
#endif
#ifndef EREMOTE
#error missing host error macro: EREMOTE
#endif
#ifndef EREMOTEIO
#error missing host error macro: EREMOTEIO
#endif
#ifndef ERESTART
#error missing host error macro: ERESTART
#endif
#ifndef ERFKILL
#error missing host error macro: ERFKILL
#endif
#ifndef EROFS
#error missing host error macro: EROFS
#endif
#ifndef ESHUTDOWN
#error missing host error macro: ESHUTDOWN
#endif
#ifndef ESOCKTNOSUPPORT
#error missing host error macro: ESOCKTNOSUPPORT
#endif
#ifndef ESPIPE
#error missing host error macro: ESPIPE
#endif
#ifndef ESRCH
#error missing host error macro: ESRCH
#endif
#ifndef ESRMNT
#error missing host error macro: ESRMNT
#endif
#ifndef ESTALE
#error missing host error macro: ESTALE
#endif
#ifndef ESTRPIPE
#error missing host error macro: ESTRPIPE
#endif
#ifndef ETIME
#error missing host error macro: ETIME
#endif
#ifndef ETIMEDOUT
#error missing host error macro: ETIMEDOUT
#endif
#ifndef ETOOMANYREFS
#error missing host error macro: ETOOMANYREFS
#endif
#ifndef ETXTBSY
#error missing host error macro: ETXTBSY
#endif
#ifndef EUCLEAN
#error missing host error macro: EUCLEAN
#endif
#ifndef EUNATCH
#error missing host error macro: EUNATCH
#endif
#ifndef EUSERS
#error missing host error macro: EUSERS
#endif
#ifndef EWOULDBLOCK
#error missing host error macro: EWOULDBLOCK
#endif
#ifndef EXDEV
#error missing host error macro: EXDEV
#endif
#ifndef EXFULL
#error missing host error macro: EXFULL
#endif
int ErrorProbe(void) {
printf("E2BIG=%d\n",E2BIG);
printf("EACCES=%d\n",EACCES);
printf("EADDRINUSE=%d\n",EADDRINUSE);
printf("EADDRNOTAVAIL=%d\n",EADDRNOTAVAIL);
printf("EADV=%d\n",EADV);
printf("EAFNOSUPPORT=%d\n",EAFNOSUPPORT);
printf("EAGAIN=%d\n",EAGAIN);
printf("EALREADY=%d\n",EALREADY);
printf("EBADE=%d\n",EBADE);
printf("EBADF=%d\n",EBADF);
printf("EBADFD=%d\n",EBADFD);
printf("EBADMSG=%d\n",EBADMSG);
printf("EBADR=%d\n",EBADR);
printf("EBADRQC=%d\n",EBADRQC);
printf("EBADSLT=%d\n",EBADSLT);
printf("EBFONT=%d\n",EBFONT);
printf("EBUSY=%d\n",EBUSY);
printf("ECANCELED=%d\n",ECANCELED);
printf("ECHILD=%d\n",ECHILD);
printf("ECHRNG=%d\n",ECHRNG);
printf("ECOMM=%d\n",ECOMM);
printf("ECONNABORTED=%d\n",ECONNABORTED);
printf("ECONNREFUSED=%d\n",ECONNREFUSED);
printf("ECONNRESET=%d\n",ECONNRESET);
printf("EDEADLK=%d\n",EDEADLK);
printf("EDEADLOCK=%d\n",EDEADLOCK);
printf("EDESTADDRREQ=%d\n",EDESTADDRREQ);
printf("EDOM=%d\n",EDOM);
printf("EDOTDOT=%d\n",EDOTDOT);
printf("EDQUOT=%d\n",EDQUOT);
printf("EEXIST=%d\n",EEXIST);
printf("EFAULT=%d\n",EFAULT);
printf("EFBIG=%d\n",EFBIG);
printf("EHOSTDOWN=%d\n",EHOSTDOWN);
printf("EHOSTUNREACH=%d\n",EHOSTUNREACH);
printf("EHWPOISON=%d\n",EHWPOISON);
printf("EIDRM=%d\n",EIDRM);
printf("EILSEQ=%d\n",EILSEQ);
printf("EINPROGRESS=%d\n",EINPROGRESS);
printf("EINTR=%d\n",EINTR);
printf("EINVAL=%d\n",EINVAL);
printf("EIO=%d\n",EIO);
printf("EISCONN=%d\n",EISCONN);
printf("EISDIR=%d\n",EISDIR);
printf("EISNAM=%d\n",EISNAM);
printf("EKEYEXPIRED=%d\n",EKEYEXPIRED);
printf("EKEYREJECTED=%d\n",EKEYREJECTED);
printf("EKEYREVOKED=%d\n",EKEYREVOKED);
printf("EL2HLT=%d\n",EL2HLT);
printf("EL2NSYNC=%d\n",EL2NSYNC);
printf("EL3HLT=%d\n",EL3HLT);
printf("EL3RST=%d\n",EL3RST);
printf("ELIBACC=%d\n",ELIBACC);
printf("ELIBBAD=%d\n",ELIBBAD);
printf("ELIBEXEC=%d\n",ELIBEXEC);
printf("ELIBMAX=%d\n",ELIBMAX);
printf("ELIBSCN=%d\n",ELIBSCN);
printf("ELNRNG=%d\n",ELNRNG);
printf("ELOOP=%d\n",ELOOP);
printf("EMEDIUMTYPE=%d\n",EMEDIUMTYPE);
printf("EMFILE=%d\n",EMFILE);
printf("EMLINK=%d\n",EMLINK);
printf("EMSGSIZE=%d\n",EMSGSIZE);
printf("EMULTIHOP=%d\n",EMULTIHOP);
printf("ENAMETOOLONG=%d\n",ENAMETOOLONG);
printf("ENAVAIL=%d\n",ENAVAIL);
printf("ENETDOWN=%d\n",ENETDOWN);
printf("ENETRESET=%d\n",ENETRESET);
printf("ENETUNREACH=%d\n",ENETUNREACH);
printf("ENFILE=%d\n",ENFILE);
printf("ENOANO=%d\n",ENOANO);
printf("ENOBUFS=%d\n",ENOBUFS);
printf("ENOCSI=%d\n",ENOCSI);
printf("ENODATA=%d\n",ENODATA);
printf("ENODEV=%d\n",ENODEV);
printf("ENOENT=%d\n",ENOENT);
printf("ENOEXEC=%d\n",ENOEXEC);
printf("ENOKEY=%d\n",ENOKEY);
printf("ENOLCK=%d\n",ENOLCK);
printf("ENOLINK=%d\n",ENOLINK);
printf("ENOMEDIUM=%d\n",ENOMEDIUM);
printf("ENOMEM=%d\n",ENOMEM);
printf("ENOMSG=%d\n",ENOMSG);
printf("ENONET=%d\n",ENONET);
printf("ENOPKG=%d\n",ENOPKG);
printf("ENOPROTOOPT=%d\n",ENOPROTOOPT);
printf("ENOSPC=%d\n",ENOSPC);
printf("ENOSR=%d\n",ENOSR);
printf("ENOSTR=%d\n",ENOSTR);
printf("ENOSYS=%d\n",ENOSYS);
printf("ENOTBLK=%d\n",ENOTBLK);
printf("ENOTCONN=%d\n",ENOTCONN);
printf("ENOTDIR=%d\n",ENOTDIR);
printf("ENOTEMPTY=%d\n",ENOTEMPTY);
printf("ENOTNAM=%d\n",ENOTNAM);
printf("ENOTRECOVERABLE=%d\n",ENOTRECOVERABLE);
printf("ENOTSOCK=%d\n",ENOTSOCK);
printf("ENOTSUP=%d\n",ENOTSUP);
printf("ENOTTY=%d\n",ENOTTY);
printf("ENOTUNIQ=%d\n",ENOTUNIQ);
printf("ENXIO=%d\n",ENXIO);
printf("EOPNOTSUPP=%d\n",EOPNOTSUPP);
printf("EOVERFLOW=%d\n",EOVERFLOW);
printf("EOWNERDEAD=%d\n",EOWNERDEAD);
printf("EPERM=%d\n",EPERM);
printf("EPFNOSUPPORT=%d\n",EPFNOSUPPORT);
printf("EPIPE=%d\n",EPIPE);
printf("EPROTO=%d\n",EPROTO);
printf("EPROTONOSUPPORT=%d\n",EPROTONOSUPPORT);
printf("EPROTOTYPE=%d\n",EPROTOTYPE);
printf("ERANGE=%d\n",ERANGE);
printf("EREMCHG=%d\n",EREMCHG);
printf("EREMOTE=%d\n",EREMOTE);
printf("EREMOTEIO=%d\n",EREMOTEIO);
printf("ERESTART=%d\n",ERESTART);
printf("ERFKILL=%d\n",ERFKILL);
printf("EROFS=%d\n",EROFS);
printf("ESHUTDOWN=%d\n",ESHUTDOWN);
printf("ESOCKTNOSUPPORT=%d\n",ESOCKTNOSUPPORT);
printf("ESPIPE=%d\n",ESPIPE);
printf("ESRCH=%d\n",ESRCH);
printf("ESRMNT=%d\n",ESRMNT);
printf("ESTALE=%d\n",ESTALE);
printf("ESTRPIPE=%d\n",ESTRPIPE);
printf("ETIME=%d\n",ETIME);
printf("ETIMEDOUT=%d\n",ETIMEDOUT);
printf("ETOOMANYREFS=%d\n",ETOOMANYREFS);
printf("ETXTBSY=%d\n",ETXTBSY);
printf("EUCLEAN=%d\n",EUCLEAN);
printf("EUNATCH=%d\n",EUNATCH);
printf("EUSERS=%d\n",EUSERS);
printf("EWOULDBLOCK=%d\n",EWOULDBLOCK);
printf("EXDEV=%d\n",EXDEV);
printf("EXFULL=%d\n",EXFULL);
return 0;
}
int ErrorSetGet(int value){errno=value;return errno;}
int ErrorRead(void){return errno;}
#ifndef BLINK_MANAGED_ERRORS
int main(void){return ErrorProbe();}
#endif
