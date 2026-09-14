#ifndef BLINK_CAMPAIGN_HOST_ERRORS_H
#define BLINK_CAMPAIGN_HOST_ERRORS_H
#include <errno.h>
/* Native-measured host error ABI; preserve the generic errno storage.
 * No OS/CPU feature identity is implied. Existing mismatches fail closed. */
#ifndef E2BIG
#define E2BIG 7
#elif E2BIG != 7
#error host errno ABI mismatch: E2BIG
#endif
#ifndef EACCES
#define EACCES 13
#elif EACCES != 13
#error host errno ABI mismatch: EACCES
#endif
#ifndef EADDRINUSE
#define EADDRINUSE 98
#elif EADDRINUSE != 98
#error host errno ABI mismatch: EADDRINUSE
#endif
#ifndef EADDRNOTAVAIL
#define EADDRNOTAVAIL 99
#elif EADDRNOTAVAIL != 99
#error host errno ABI mismatch: EADDRNOTAVAIL
#endif
#ifndef EADV
#define EADV 68
#elif EADV != 68
#error host errno ABI mismatch: EADV
#endif
#ifndef EAFNOSUPPORT
#define EAFNOSUPPORT 97
#elif EAFNOSUPPORT != 97
#error host errno ABI mismatch: EAFNOSUPPORT
#endif
#ifndef EAGAIN
#define EAGAIN 11
#elif EAGAIN != 11
#error host errno ABI mismatch: EAGAIN
#endif
#ifndef EALREADY
#define EALREADY 114
#elif EALREADY != 114
#error host errno ABI mismatch: EALREADY
#endif
#ifndef EBADE
#define EBADE 52
#elif EBADE != 52
#error host errno ABI mismatch: EBADE
#endif
#ifndef EBADF
#define EBADF 9
#elif EBADF != 9
#error host errno ABI mismatch: EBADF
#endif
#ifndef EBADFD
#define EBADFD 77
#elif EBADFD != 77
#error host errno ABI mismatch: EBADFD
#endif
#ifndef EBADMSG
#define EBADMSG 74
#elif EBADMSG != 74
#error host errno ABI mismatch: EBADMSG
#endif
#ifndef EBADR
#define EBADR 53
#elif EBADR != 53
#error host errno ABI mismatch: EBADR
#endif
#ifndef EBADRQC
#define EBADRQC 56
#elif EBADRQC != 56
#error host errno ABI mismatch: EBADRQC
#endif
#ifndef EBADSLT
#define EBADSLT 57
#elif EBADSLT != 57
#error host errno ABI mismatch: EBADSLT
#endif
#ifndef EBFONT
#define EBFONT 59
#elif EBFONT != 59
#error host errno ABI mismatch: EBFONT
#endif
#ifndef EBUSY
#define EBUSY 16
#elif EBUSY != 16
#error host errno ABI mismatch: EBUSY
#endif
#ifndef ECANCELED
#define ECANCELED 125
#elif ECANCELED != 125
#error host errno ABI mismatch: ECANCELED
#endif
#ifndef ECHILD
#define ECHILD 10
#elif ECHILD != 10
#error host errno ABI mismatch: ECHILD
#endif
#ifndef ECHRNG
#define ECHRNG 44
#elif ECHRNG != 44
#error host errno ABI mismatch: ECHRNG
#endif
#ifndef ECOMM
#define ECOMM 70
#elif ECOMM != 70
#error host errno ABI mismatch: ECOMM
#endif
#ifndef ECONNABORTED
#define ECONNABORTED 103
#elif ECONNABORTED != 103
#error host errno ABI mismatch: ECONNABORTED
#endif
#ifndef ECONNREFUSED
#define ECONNREFUSED 111
#elif ECONNREFUSED != 111
#error host errno ABI mismatch: ECONNREFUSED
#endif
#ifndef ECONNRESET
#define ECONNRESET 104
#elif ECONNRESET != 104
#error host errno ABI mismatch: ECONNRESET
#endif
#ifndef EDEADLK
#define EDEADLK 35
#elif EDEADLK != 35
#error host errno ABI mismatch: EDEADLK
#endif
#ifndef EDEADLOCK
#define EDEADLOCK 35
#elif EDEADLOCK != 35
#error host errno ABI mismatch: EDEADLOCK
#endif
#ifndef EDESTADDRREQ
#define EDESTADDRREQ 89
#elif EDESTADDRREQ != 89
#error host errno ABI mismatch: EDESTADDRREQ
#endif
#ifndef EDOM
#define EDOM 33
#elif EDOM != 33
#error host errno ABI mismatch: EDOM
#endif
#ifndef EDOTDOT
#define EDOTDOT 73
#elif EDOTDOT != 73
#error host errno ABI mismatch: EDOTDOT
#endif
#ifndef EDQUOT
#define EDQUOT 122
#elif EDQUOT != 122
#error host errno ABI mismatch: EDQUOT
#endif
#ifndef EEXIST
#define EEXIST 17
#elif EEXIST != 17
#error host errno ABI mismatch: EEXIST
#endif
#ifndef EFAULT
#define EFAULT 14
#elif EFAULT != 14
#error host errno ABI mismatch: EFAULT
#endif
#ifndef EFBIG
#define EFBIG 27
#elif EFBIG != 27
#error host errno ABI mismatch: EFBIG
#endif
#ifndef EHOSTDOWN
#define EHOSTDOWN 112
#elif EHOSTDOWN != 112
#error host errno ABI mismatch: EHOSTDOWN
#endif
#ifndef EHOSTUNREACH
#define EHOSTUNREACH 113
#elif EHOSTUNREACH != 113
#error host errno ABI mismatch: EHOSTUNREACH
#endif
#ifndef EHWPOISON
#define EHWPOISON 133
#elif EHWPOISON != 133
#error host errno ABI mismatch: EHWPOISON
#endif
#ifndef EIDRM
#define EIDRM 43
#elif EIDRM != 43
#error host errno ABI mismatch: EIDRM
#endif
#ifndef EILSEQ
#define EILSEQ 84
#elif EILSEQ != 84
#error host errno ABI mismatch: EILSEQ
#endif
#ifndef EINPROGRESS
#define EINPROGRESS 115
#elif EINPROGRESS != 115
#error host errno ABI mismatch: EINPROGRESS
#endif
#ifndef EINTR
#define EINTR 4
#elif EINTR != 4
#error host errno ABI mismatch: EINTR
#endif
#ifndef EINVAL
#define EINVAL 22
#elif EINVAL != 22
#error host errno ABI mismatch: EINVAL
#endif
#ifndef EIO
#define EIO 5
#elif EIO != 5
#error host errno ABI mismatch: EIO
#endif
#ifndef EISCONN
#define EISCONN 106
#elif EISCONN != 106
#error host errno ABI mismatch: EISCONN
#endif
#ifndef EISDIR
#define EISDIR 21
#elif EISDIR != 21
#error host errno ABI mismatch: EISDIR
#endif
#ifndef EISNAM
#define EISNAM 120
#elif EISNAM != 120
#error host errno ABI mismatch: EISNAM
#endif
#ifndef EKEYEXPIRED
#define EKEYEXPIRED 127
#elif EKEYEXPIRED != 127
#error host errno ABI mismatch: EKEYEXPIRED
#endif
#ifndef EKEYREJECTED
#define EKEYREJECTED 129
#elif EKEYREJECTED != 129
#error host errno ABI mismatch: EKEYREJECTED
#endif
#ifndef EKEYREVOKED
#define EKEYREVOKED 128
#elif EKEYREVOKED != 128
#error host errno ABI mismatch: EKEYREVOKED
#endif
#ifndef EL2HLT
#define EL2HLT 51
#elif EL2HLT != 51
#error host errno ABI mismatch: EL2HLT
#endif
#ifndef EL2NSYNC
#define EL2NSYNC 45
#elif EL2NSYNC != 45
#error host errno ABI mismatch: EL2NSYNC
#endif
#ifndef EL3HLT
#define EL3HLT 46
#elif EL3HLT != 46
#error host errno ABI mismatch: EL3HLT
#endif
#ifndef EL3RST
#define EL3RST 47
#elif EL3RST != 47
#error host errno ABI mismatch: EL3RST
#endif
#ifndef ELIBACC
#define ELIBACC 79
#elif ELIBACC != 79
#error host errno ABI mismatch: ELIBACC
#endif
#ifndef ELIBBAD
#define ELIBBAD 80
#elif ELIBBAD != 80
#error host errno ABI mismatch: ELIBBAD
#endif
#ifndef ELIBEXEC
#define ELIBEXEC 83
#elif ELIBEXEC != 83
#error host errno ABI mismatch: ELIBEXEC
#endif
#ifndef ELIBMAX
#define ELIBMAX 82
#elif ELIBMAX != 82
#error host errno ABI mismatch: ELIBMAX
#endif
#ifndef ELIBSCN
#define ELIBSCN 81
#elif ELIBSCN != 81
#error host errno ABI mismatch: ELIBSCN
#endif
#ifndef ELNRNG
#define ELNRNG 48
#elif ELNRNG != 48
#error host errno ABI mismatch: ELNRNG
#endif
#ifndef ELOOP
#define ELOOP 40
#elif ELOOP != 40
#error host errno ABI mismatch: ELOOP
#endif
#ifndef EMEDIUMTYPE
#define EMEDIUMTYPE 124
#elif EMEDIUMTYPE != 124
#error host errno ABI mismatch: EMEDIUMTYPE
#endif
#ifndef EMFILE
#define EMFILE 24
#elif EMFILE != 24
#error host errno ABI mismatch: EMFILE
#endif
#ifndef EMLINK
#define EMLINK 31
#elif EMLINK != 31
#error host errno ABI mismatch: EMLINK
#endif
#ifndef EMSGSIZE
#define EMSGSIZE 90
#elif EMSGSIZE != 90
#error host errno ABI mismatch: EMSGSIZE
#endif
#ifndef EMULTIHOP
#define EMULTIHOP 72
#elif EMULTIHOP != 72
#error host errno ABI mismatch: EMULTIHOP
#endif
#ifndef ENAMETOOLONG
#define ENAMETOOLONG 36
#elif ENAMETOOLONG != 36
#error host errno ABI mismatch: ENAMETOOLONG
#endif
#ifndef ENAVAIL
#define ENAVAIL 119
#elif ENAVAIL != 119
#error host errno ABI mismatch: ENAVAIL
#endif
#ifndef ENETDOWN
#define ENETDOWN 100
#elif ENETDOWN != 100
#error host errno ABI mismatch: ENETDOWN
#endif
#ifndef ENETRESET
#define ENETRESET 102
#elif ENETRESET != 102
#error host errno ABI mismatch: ENETRESET
#endif
#ifndef ENETUNREACH
#define ENETUNREACH 101
#elif ENETUNREACH != 101
#error host errno ABI mismatch: ENETUNREACH
#endif
#ifndef ENFILE
#define ENFILE 23
#elif ENFILE != 23
#error host errno ABI mismatch: ENFILE
#endif
#ifndef ENOANO
#define ENOANO 55
#elif ENOANO != 55
#error host errno ABI mismatch: ENOANO
#endif
#ifndef ENOBUFS
#define ENOBUFS 105
#elif ENOBUFS != 105
#error host errno ABI mismatch: ENOBUFS
#endif
#ifndef ENOCSI
#define ENOCSI 50
#elif ENOCSI != 50
#error host errno ABI mismatch: ENOCSI
#endif
#ifndef ENODATA
#define ENODATA 61
#elif ENODATA != 61
#error host errno ABI mismatch: ENODATA
#endif
#ifndef ENODEV
#define ENODEV 19
#elif ENODEV != 19
#error host errno ABI mismatch: ENODEV
#endif
#ifndef ENOENT
#define ENOENT 2
#elif ENOENT != 2
#error host errno ABI mismatch: ENOENT
#endif
#ifndef ENOEXEC
#define ENOEXEC 8
#elif ENOEXEC != 8
#error host errno ABI mismatch: ENOEXEC
#endif
#ifndef ENOKEY
#define ENOKEY 126
#elif ENOKEY != 126
#error host errno ABI mismatch: ENOKEY
#endif
#ifndef ENOLCK
#define ENOLCK 37
#elif ENOLCK != 37
#error host errno ABI mismatch: ENOLCK
#endif
#ifndef ENOLINK
#define ENOLINK 67
#elif ENOLINK != 67
#error host errno ABI mismatch: ENOLINK
#endif
#ifndef ENOMEDIUM
#define ENOMEDIUM 123
#elif ENOMEDIUM != 123
#error host errno ABI mismatch: ENOMEDIUM
#endif
#ifndef ENOMEM
#define ENOMEM 12
#elif ENOMEM != 12
#error host errno ABI mismatch: ENOMEM
#endif
#ifndef ENOMSG
#define ENOMSG 42
#elif ENOMSG != 42
#error host errno ABI mismatch: ENOMSG
#endif
#ifndef ENONET
#define ENONET 64
#elif ENONET != 64
#error host errno ABI mismatch: ENONET
#endif
#ifndef ENOPKG
#define ENOPKG 65
#elif ENOPKG != 65
#error host errno ABI mismatch: ENOPKG
#endif
#ifndef ENOPROTOOPT
#define ENOPROTOOPT 92
#elif ENOPROTOOPT != 92
#error host errno ABI mismatch: ENOPROTOOPT
#endif
#ifndef ENOSPC
#define ENOSPC 28
#elif ENOSPC != 28
#error host errno ABI mismatch: ENOSPC
#endif
#ifndef ENOSR
#define ENOSR 63
#elif ENOSR != 63
#error host errno ABI mismatch: ENOSR
#endif
#ifndef ENOSTR
#define ENOSTR 60
#elif ENOSTR != 60
#error host errno ABI mismatch: ENOSTR
#endif
#ifndef ENOSYS
#define ENOSYS 38
#elif ENOSYS != 38
#error host errno ABI mismatch: ENOSYS
#endif
#ifndef ENOTBLK
#define ENOTBLK 15
#elif ENOTBLK != 15
#error host errno ABI mismatch: ENOTBLK
#endif
#ifndef ENOTCONN
#define ENOTCONN 107
#elif ENOTCONN != 107
#error host errno ABI mismatch: ENOTCONN
#endif
#ifndef ENOTDIR
#define ENOTDIR 20
#elif ENOTDIR != 20
#error host errno ABI mismatch: ENOTDIR
#endif
#ifndef ENOTEMPTY
#define ENOTEMPTY 39
#elif ENOTEMPTY != 39
#error host errno ABI mismatch: ENOTEMPTY
#endif
#ifndef ENOTNAM
#define ENOTNAM 118
#elif ENOTNAM != 118
#error host errno ABI mismatch: ENOTNAM
#endif
#ifndef ENOTRECOVERABLE
#define ENOTRECOVERABLE 131
#elif ENOTRECOVERABLE != 131
#error host errno ABI mismatch: ENOTRECOVERABLE
#endif
#ifndef ENOTSOCK
#define ENOTSOCK 88
#elif ENOTSOCK != 88
#error host errno ABI mismatch: ENOTSOCK
#endif
#ifndef ENOTSUP
#define ENOTSUP 95
#elif ENOTSUP != 95
#error host errno ABI mismatch: ENOTSUP
#endif
#ifndef ENOTTY
#define ENOTTY 25
#elif ENOTTY != 25
#error host errno ABI mismatch: ENOTTY
#endif
#ifndef ENOTUNIQ
#define ENOTUNIQ 76
#elif ENOTUNIQ != 76
#error host errno ABI mismatch: ENOTUNIQ
#endif
#ifndef ENXIO
#define ENXIO 6
#elif ENXIO != 6
#error host errno ABI mismatch: ENXIO
#endif
#ifndef EOPNOTSUPP
#define EOPNOTSUPP 95
#elif EOPNOTSUPP != 95
#error host errno ABI mismatch: EOPNOTSUPP
#endif
#ifndef EOVERFLOW
#define EOVERFLOW 75
#elif EOVERFLOW != 75
#error host errno ABI mismatch: EOVERFLOW
#endif
#ifndef EOWNERDEAD
#define EOWNERDEAD 130
#elif EOWNERDEAD != 130
#error host errno ABI mismatch: EOWNERDEAD
#endif
#ifndef EPERM
#define EPERM 1
#elif EPERM != 1
#error host errno ABI mismatch: EPERM
#endif
#ifndef EPFNOSUPPORT
#define EPFNOSUPPORT 96
#elif EPFNOSUPPORT != 96
#error host errno ABI mismatch: EPFNOSUPPORT
#endif
#ifndef EPIPE
#define EPIPE 32
#elif EPIPE != 32
#error host errno ABI mismatch: EPIPE
#endif
#ifndef EPROTO
#define EPROTO 71
#elif EPROTO != 71
#error host errno ABI mismatch: EPROTO
#endif
#ifndef EPROTONOSUPPORT
#define EPROTONOSUPPORT 93
#elif EPROTONOSUPPORT != 93
#error host errno ABI mismatch: EPROTONOSUPPORT
#endif
#ifndef EPROTOTYPE
#define EPROTOTYPE 91
#elif EPROTOTYPE != 91
#error host errno ABI mismatch: EPROTOTYPE
#endif
#ifndef ERANGE
#define ERANGE 34
#elif ERANGE != 34
#error host errno ABI mismatch: ERANGE
#endif
#ifndef EREMCHG
#define EREMCHG 78
#elif EREMCHG != 78
#error host errno ABI mismatch: EREMCHG
#endif
#ifndef EREMOTE
#define EREMOTE 66
#elif EREMOTE != 66
#error host errno ABI mismatch: EREMOTE
#endif
#ifndef EREMOTEIO
#define EREMOTEIO 121
#elif EREMOTEIO != 121
#error host errno ABI mismatch: EREMOTEIO
#endif
#ifndef ERESTART
#define ERESTART 85
#elif ERESTART != 85
#error host errno ABI mismatch: ERESTART
#endif
#ifndef ERFKILL
#define ERFKILL 132
#elif ERFKILL != 132
#error host errno ABI mismatch: ERFKILL
#endif
#ifndef EROFS
#define EROFS 30
#elif EROFS != 30
#error host errno ABI mismatch: EROFS
#endif
#ifndef ESHUTDOWN
#define ESHUTDOWN 108
#elif ESHUTDOWN != 108
#error host errno ABI mismatch: ESHUTDOWN
#endif
#ifndef ESOCKTNOSUPPORT
#define ESOCKTNOSUPPORT 94
#elif ESOCKTNOSUPPORT != 94
#error host errno ABI mismatch: ESOCKTNOSUPPORT
#endif
#ifndef ESPIPE
#define ESPIPE 29
#elif ESPIPE != 29
#error host errno ABI mismatch: ESPIPE
#endif
#ifndef ESRCH
#define ESRCH 3
#elif ESRCH != 3
#error host errno ABI mismatch: ESRCH
#endif
#ifndef ESRMNT
#define ESRMNT 69
#elif ESRMNT != 69
#error host errno ABI mismatch: ESRMNT
#endif
#ifndef ESTALE
#define ESTALE 116
#elif ESTALE != 116
#error host errno ABI mismatch: ESTALE
#endif
#ifndef ESTRPIPE
#define ESTRPIPE 86
#elif ESTRPIPE != 86
#error host errno ABI mismatch: ESTRPIPE
#endif
#ifndef ETIME
#define ETIME 62
#elif ETIME != 62
#error host errno ABI mismatch: ETIME
#endif
#ifndef ETIMEDOUT
#define ETIMEDOUT 110
#elif ETIMEDOUT != 110
#error host errno ABI mismatch: ETIMEDOUT
#endif
#ifndef ETOOMANYREFS
#define ETOOMANYREFS 109
#elif ETOOMANYREFS != 109
#error host errno ABI mismatch: ETOOMANYREFS
#endif
#ifndef ETXTBSY
#define ETXTBSY 26
#elif ETXTBSY != 26
#error host errno ABI mismatch: ETXTBSY
#endif
#ifndef EUCLEAN
#define EUCLEAN 117
#elif EUCLEAN != 117
#error host errno ABI mismatch: EUCLEAN
#endif
#ifndef EUNATCH
#define EUNATCH 49
#elif EUNATCH != 49
#error host errno ABI mismatch: EUNATCH
#endif
#ifndef EUSERS
#define EUSERS 87
#elif EUSERS != 87
#error host errno ABI mismatch: EUSERS
#endif
#ifndef EWOULDBLOCK
#define EWOULDBLOCK 11
#elif EWOULDBLOCK != 11
#error host errno ABI mismatch: EWOULDBLOCK
#endif
#ifndef EXDEV
#define EXDEV 18
#elif EXDEV != 18
#error host errno ABI mismatch: EXDEV
#endif
#ifndef EXFULL
#define EXFULL 54
#elif EXFULL != 54
#error host errno ABI mismatch: EXFULL
#endif
#endif
