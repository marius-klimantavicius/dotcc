#pragma once
#include <stdint.h>
#include "api/s2n.h"
struct s2n_security_policy;
struct dotcc_s2n_policy;

/* The library-global policy has static lifetime. A narrowed policy is owned by
 * its caller and must outlive every connection to which it has been assigned. */
const struct s2n_security_policy *dotcc_s2n_compatibility_policy(void);
int dotcc_s2n_config_use_compat(struct s2n_config *config);
struct dotcc_s2n_policy *dotcc_s2n_policy_new(uint8_t minimum, uint8_t maximum, uint16_t cipher);
int dotcc_s2n_connection_use_policy(struct s2n_connection *conn, struct dotcc_s2n_policy *owner);
void dotcc_s2n_policy_free(struct dotcc_s2n_policy *owner);
