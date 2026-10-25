/* Ordinary Linux futex synchronization: absolute deadlines and mixed masks. */
.global _start
.text
_start:
#ifdef CHANGED_ONLY
    jmp changedphase
#endif
    lea word(%rip),%rdi
    mov $137,%esi
    mov $1,%edx
    xor %r10d,%r10d
    mov $-1,%r9d
    mov $202,%eax
    syscall
    cmp $-11,%rax
    jne fail
    xor %r12d,%r12d             /* CLOCK_REALTIME absolute wait */
    call timedwait
    mov $1,%r12d               /* CLOCK_MONOTONIC absolute wait */
    call timedwait
    lea stacks_end(%rip),%r14
    lea tids(%rip),%r15
    mov $4,%r13d
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
    cmpl $4,ready(%rip)
    je settle
    mov $24,%eax
    syscall
    jmp readywait
settle:
    call pause
    /* Normal WAIT has MATCH_ANY, so this mask selects only that waiter. */
    mov $8,%r9d
    mov $3,%edx
    call wakebitset
    cmp $1,%rax
    jne fail
    mov $2,%r9d
    mov $1,%edx
    call wakebitset
    cmp $1,%rax
    jne fail
    call wakebitset
    test %rax,%rax
    jne fail
    mov $5,%r9d
    call wakebitset
    cmp $1,%rax
    jne fail
    /* Ordinary WAKE can select the remaining BITSET waiter. */
    lea word(%rip),%rdi
    mov $129,%esi
    mov $1,%edx
    mov $202,%eax
    syscall
    cmp $1,%rax
    jne fail
    mov $-1,%r9d
    call wakebitset
    test %rax,%rax
    jne fail
    lea tids(%rip),%r12
    mov $4,%r13d
join:
    call joinone
    add $4,%r12
    dec %r13d
    jnz join
    cmpl $4,done(%rip)
    jne fail
#ifdef MASKS_ONLY
    xor %edi,%edi
    jmp exit
#endif
changedphase:
    /* A store without FUTEX_WAKE must leave a queued waiter until timeout. */
    movl $0,ready(%rip)
    mov $5,%r13d
    lea stacks_end(%rip),%rsi
    lea tids(%rip),%r10
    mov %r10,%rdx
    xor %r8d,%r8d
    mov $0x310f00,%edi
    mov $56,%eax
    syscall
    test %rax,%rax
    jz child
    js fail
waitchanged:
    cmpl $1,ready(%rip)
    je change
    mov $24,%eax
    syscall
    jmp waitchanged
change:
    call pause
    movl $1,word(%rip)
    lea tids(%rip),%r12
    call joinone
#ifdef CHANGED_ONLY
    cmpl $1,done(%rip)
#else
    cmpl $5,done(%rip)
#endif
    jne fail
    xor %edi,%edi
    jmp exit
child:
    cmp $5,%r13d
    je changedchild
    mov $1,%r9d
    mov %r13d,%ecx
    sub $1,%ecx
    shl %cl,%r9d
    mov $137,%esi
    cmp $4,%r13d
    jne childready
    mov $128,%esi
childready:
    lock incl ready(%rip)
    lea word(%rip),%rdi
    xor %edx,%edx
    xor %r10d,%r10d
    mov $202,%eax
    syscall
    test %rax,%rax
    jne fail
childdone:
    lock incl done(%rip)
    xor %edi,%edi
    mov $60,%eax
    syscall
    jmp fail
changedchild:
    mov $1,%edi
    lea deadline(%rip),%rsi
    mov $228,%eax
    syscall
    test %rax,%rax
    jne fail
    addq $400000000,deadline+8(%rip)
    call normalize
    lock incl ready(%rip)
    lea word(%rip),%rdi
    mov $137,%esi
    xor %edx,%edx
    lea deadline(%rip),%r10
    mov $-1,%r9d
    mov $202,%eax
    syscall
    cmp $-110,%rax
    jne fail
    jmp childdone
joinone:
    mov (%r12),%edx
    test %edx,%edx
    jz joined
    mov %r12,%rdi
    xor %esi,%esi
    xor %r10d,%r10d
    mov $202,%eax
    syscall
    test %rax,%rax
    je joinone
    cmp $-11,%rax
    je joinone
    cmp $-4,%rax
    je joinone
    jmp fail
joined:
    ret
pause:
    mov $35,%eax
    lea delay(%rip),%rdi
    xor %esi,%esi
    syscall
    test %rax,%rax
    jne fail
    ret
wakebitset:
    lea word(%rip),%rdi
    mov $138,%esi
    mov $202,%eax
    syscall
    ret
timedwait:
    mov %r12d,%edi
    lea deadline(%rip),%rsi
    mov $228,%eax
    syscall
    test %rax,%rax
    jne fail
    addq $30000000,deadline+8(%rip)
    call normalize
    lea word(%rip),%rdi
    mov $137,%esi
    test %r12d,%r12d
    jnz monotonic
    or $256,%esi
monotonic:
    xor %edx,%edx
    lea deadline(%rip),%r10
    mov $-1,%r9d
    mov $202,%eax
    syscall
    cmp $-110,%rax
    jne fail
    mov %r12d,%edi
    lea after(%rip),%rsi
    mov $228,%eax
    syscall
    test %rax,%rax
    jne fail
    mov after(%rip),%rax
    sub deadline(%rip),%rax
    js fail
    cmp $2,%rax                /* catches monotonic interpreted as relative */
    ja fail
    test %rax,%rax
    jnz timedone
    mov after+8(%rip),%rax
    cmp deadline+8(%rip),%rax
    jb fail
timedone:
    ret
normalize:
    cmpq $1000000000,deadline+8(%rip)
    jb normalized
    subq $1000000000,deadline+8(%rip)
    incq deadline(%rip)
normalized:
    ret
fail:
    mov $99,%edi
exit:
    mov $231,%eax
    syscall
.data
.balign 8
delay: .quad 0,100000000
.bss
.balign 4096
stacks: .skip 262144
stacks_end:
word: .long 0
ready: .long 0
done: .long 0
tids: .skip 16
.balign 8
deadline: .skip 16
after: .skip 16
.section .note.GNU-stack,"",@progbits
