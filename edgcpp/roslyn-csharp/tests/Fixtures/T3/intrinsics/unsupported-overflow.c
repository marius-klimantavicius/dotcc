static unsigned int left = 0xffffffffU;
static unsigned int right = 1U;
int fixture(void)
{
  unsigned int result;
  return __builtin_add_overflow(left, right, &result);
}
