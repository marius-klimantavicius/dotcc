#ifndef DOTCC_MANAGED_GSSAPI_H
#define DOTCC_MANAGED_GSSAPI_H

#include <stddef.h>
#include <stdint.h>

typedef uint32_t OM_uint32;
typedef void *gss_ctx_id_t;
typedef void *gss_cred_id_t;
typedef void *gss_name_t;

typedef struct gss_OID_desc_struct {
        OM_uint32 length;
        void *elements;
} gss_OID_desc;

typedef gss_OID_desc *gss_OID;

typedef const gss_OID_desc *gss_const_OID;

typedef struct gss_buffer_desc_struct {
        size_t length;
        void *value;
} gss_buffer_desc;

OM_uint32 gss_release_cred(OM_uint32 *minor_status,
                           gss_cred_id_t *cred_handle);

#endif
