//type: fp
//options: 
# 0 "./auto-init-uninit-21.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./auto-init-uninit-21.c"



# 1 "./uninit-21.c" 1




enum clnt_stat {
 RPC_SUCCESS=0,
 RPC_CANTENCODEARGS=1,
};

int do_ypcall_tr ();

static int
yp_master (char **outname)
{

  enum clnt_stat result;
  result = do_ypcall_tr ();
  if (result != 0)
    return result;
  *outname = __builtin_strdup ("foo");
  return 0;
}

int
yp_update (void)
{
  char *master;
  int r;
  if ((r = yp_master (&master)) != 0)
    return r;
  __builtin_free (master);
  return 0;
}
# 5 "./auto-init-uninit-21.c" 2
