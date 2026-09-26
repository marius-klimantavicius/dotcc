using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LALR.CC;
using LALR.CC.LexicalGrammar;
using LALR.CC.LexicalGrammar.Dfa;

namespace DotCC;

/// <summary>Compile a grammar's UTF-8 automata once. LALR.CC's BytesLexer
/// constructor recompiles them for every input, including individual macros.
/// Only immutable tables are shared; each stream owns its cursor and state stack.
/// Uses dotcc's default lexer settings: throw on errors, codepoint columns.</summary>
internal sealed class LexerGrammar
{
    private sealed record State(Dfa Automaton, LexRule[] Rules);
    private readonly Dictionary<string, State> _states = new(StringComparer.Ordinal);
    private static readonly Lazy<LexerGrammar> CGrammar = new(() => new(DotCC.C.BuildLexer()));
    private static readonly Lazy<LexerGrammar> ZigGrammar = new(() => new(DotCC.Zig.BuildLexer()));
    internal static LexerGrammar C => CGrammar.Value;
    internal static LexerGrammar Zig => ZigGrammar.Value;

    internal LexerGrammar(IReadOnlyDictionary<string, LexRule[]> rules)
    {
        if (!rules.ContainsKey("root")) throw new ArgumentException("Lexer requires a root state", nameof(rules));
        foreach (var (name, patterns) in rules)
        {
            if (patterns.Length == 0) throw new ArgumentException("Empty lexer state: " + name, nameof(rules));
            var owned = (LexRule[])patterns.Clone();
            var automaton = Utf8DfaLowering.Lower(DfaCompiler.CompileMany(
                owned.Select((rule, index) => (rule.Pattern, index)).ToArray()));
            _states.Add(name, new(automaton, owned));
        }
    }

    internal ISyncIterator<Item> FromString(string text, int initialLine = 1)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentOutOfRangeException.ThrowIfLessThan(initialLine, 1);
        return new Stream(this, Encoding.UTF8.GetBytes(text), initialLine);
    }

    // Match the byte lexer's longest-match/rule-order behavior and codepoint
    // columns, including UTF-8 byte offsets used by physical source mapping.
    private sealed class Stream(LexerGrammar grammar, byte[] bytes, int initialLine) : ISyncIterator<Item>
    {
        private readonly Stack<string> _states = new(["root"]);
        private int _offset;
        private int _line = initialLine;
        private int _column = 1;
        private Item? _current;
        public Item Current => _current ?? throw new InvalidOperationException("Did not call MoveNext() yet!");
        public bool SupportsResetting => false;

        public bool MoveNext()
        {
            while (_offset < bytes.Length)
            {
                var stateName = _states.Peek();
                var state = grammar._states[stateName];
                var automaton = state.Automaton;
                var node = automaton.Start;
                int length = 0, accept = -1;
                for (var index = _offset; index < bytes.Length; ++index)
                {
                    node = automaton.Step(node, bytes[index]);
                    if (node < 0) break;
                    if (automaton.States[node].IsAccept)
                    {
                        length = index - _offset + 1;
                        accept = automaton.States[node].Accept;
                    }
                }
                var position = new SourcePosition(_line, _column, _offset);
                if (length == 0)
                {
                    var invalid = bytes[_offset];
                    _offset = bytes.Length;
                    throw new LexerException(position, invalid, stateName);
                }
                var rule = state.Rules[accept];
                var text = Encoding.UTF8.GetString(bytes.AsSpan(_offset, length));
                foreach (var value in bytes.AsSpan(_offset, length))
                {
                    if (value == '\n') { ++_line; _column = 1; }
                    else if ((value & 0xC0) != 0x80) ++_column;
                }
                _offset += length;
                switch (rule.Instruction)
                {
                    case null or "" or "#ignore": break;
                    case "#pop": _states.Pop(); break;
                    case var next:
                        if (!grammar._states.ContainsKey(next))
                            throw new InvalidOperationException("lexer rule pushed unknown state '" + next + "'");
                        _states.Push(next);
                        break;
                }
                if (rule.Instruction == "#ignore") continue;
                _current = new Item(rule.SymbolId, text, position);
                return true;
            }
            return false;
        }

        public void Reset() => throw new InvalidOperationException("Resetting is not supported");
        public void Dispose() { }
    }
}
