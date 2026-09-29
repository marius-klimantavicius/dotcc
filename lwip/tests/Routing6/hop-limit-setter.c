#include <stdio.h>
#include <stdlib.h>
#include "hop-limit-override.h"
static struct ip6_hdr header;static unsigned headers,values;
static struct ip6_hdr*next_header(void){headers++;return &header;}
static int next_value(void){values++;return -1;}
int main(void)
{
  for(int i=0;i<=255;i++){int result=IP6H_HOPLIM_SET(&header,i);if(IP6H_HOPLIM(&header)!=i||result!=i)return 92;}
  int result=IP6H_HOPLIM_SET(next_header(),next_value());
  if(headers!=1||values!=1||result!=0||IP6H_HOPLIM(&header)!=0)return 92;
  puts("{\"case\":\"hop-limit-setter\",\"valid_values\":256,\"single_evaluation\":true,\"assignment_result\":true}");return 0;
}
