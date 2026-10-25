/* Three real waiters. Back-to-back wakes include still-dequeuing waiters. */
.global _start
.text
_start:
    lea word(%rip),%rdi
    mov $128,%esi
    mov $1,%edx
    xor %r10d,%r10d
    mov $202,%eax
    syscall
    cmp $-11,%rax               /* changed-value mismatch never queues */
    jne fail
    lea word(%rip),%rdi
    mov $128,%esi
    xor %edx,%edx
    lea timeout(%rip),%r10
    mov $202,%eax
    syscall
    cmp $-110,%rax              /* ordinary timed wait and deregistration */
    jne fail
    mov $0x7fffffff,%edx
    call wake
    test %rax,%rax
    jne fail
    lea stacks_end(%rip),%r14
    lea tids(%rip),%r15
    mov $3,%r13d
spawn:
    mov $56,%eax
    mov $0x310f00,%edi
    mov %r14,%rsi
    mov %r15,%rdx
    mov %r15,%r10
    xor %r8d,%r8d
    syscall
    test %rax,%rax
    jz child
    js fail
    add $4,%r15
    sub $65536,%r14
    dec %r13d
    jnz spawn
readywait:
    cmpl $3,ready(%rip)
    je settle
    mov $24,%eax
    syscall
    jmp readywait
settle:
    mov $35,%eax
    lea delay(%rip),%rdi
    xor %esi,%esi
    syscall
    test %rax,%rax
    jne fail
#ifdef PROFILE
    xor %edx,%edx               /* retain the profile's zero-count no-op */
    call wake
    test %rax,%rax
    jne fail
#endif
    mov $2,%edx
    call wake
    cmp $2,%rax
    jne over_wake
    mov $1,%edx
    call wake
    cmp $1,%rax
    jne fail
    mov $0x7fffffff,%edx
    call wake
    test %rax,%rax
    jne fail
    lea tids(%rip),%r12
    mov $3,%r13d
join:
    mov (%r12),%edx
    test %edx,%edx
    jz joined
    mov %r12,%rdi
    xor %esi,%esi
    xor %r10d,%r10d
    mov $202,%eax
    syscall
    test %rax,%rax
    je join
    cmp $-11,%rax
    je join
    cmp $-4,%rax
    je join
    jmp fail
joined:
    add $4,%r12
    dec %r13d
    jnz join
    cmpl $3,done(%rip)
    jne fail
    xor %edi,%edi
    jmp exit
child:
    lock incl ready(%rip)
    lea word(%rip),%rdi
    mov $128,%esi
    xor %edx,%edx
    xor %r10d,%r10d
    mov $202,%eax
    syscall
    test %rax,%rax
    jne fail
    lock incl done(%rip)
    xor %edi,%edi
    mov $60,%eax
    syscall
    jmp fail
wake:
    lea word(%rip),%rdi
    mov $129,%esi
    mov $202,%eax
    syscall
    ret
over_wake:
    mov $42,%edi
    jmp exit
fail:
    mov $99,%edi
exit:
    mov $231,%eax
    syscall
.data
.balign 8
delay: .quad 0,100000000
timeout: .quad 0,1000000
.bss
.balign 4096
stacks: .skip 196608
stacks_end:
word: .long 0
ready: .long 0
done: .long 0
tids: .skip 12
.section .note.GNU-stack,"",@progbits
