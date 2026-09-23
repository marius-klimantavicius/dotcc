#ifndef DOTCC_MANAGED_KRB5_H
#define DOTCC_MANAGED_KRB5_H

typedef void *krb5_context;
typedef void *krb5_ccache;
typedef void *krb5_principal;
typedef void *krb5_keytab;
typedef int krb5_error_code;

typedef struct krb5_creds {
        void *managed_state;
} krb5_creds;

#endif
