.text
.global _start, memory_restorer, faulting_store, store_fault_site, store_resume, recovery_ret
_start:
  and $-16,%rsp
  call guest_main
  mov %eax,%edi
  mov $231,%eax
  syscall
  jmp .
memory_restorer:
  mov $15,%eax
  syscall
  ud2
faulting_store:
  movabs $0x1122334455667788,%rax
store_fault_site:
  mov %rax,(%rdi)
store_resume:
  ret
recovery_ret:
  ret
.section .note.GNU-stack,"",@progbits
