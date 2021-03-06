using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using KaLib.Brigadier.Builder;
using KaLib.Brigadier.Context;
using KaLib.Brigadier.Exceptions;
using KaLib.Brigadier.Suggests;

namespace KaLib.Brigadier.Tree
{
    public class LiteralCommandNode<TS> : CommandNode<TS> {
        private readonly string _literal;
        private readonly string _literalLowerCase;

        public LiteralCommandNode(string literal, ICommand<TS> command, Predicate<TS> requirement, CommandNode<TS> redirect, RedirectModifier<TS> modifier, bool forks) 
            : base(command, requirement, redirect, modifier, forks) {
            this._literal = literal;
            this._literalLowerCase = literal.ToLower(CultureInfo.InvariantCulture);
        }

        public string GetLiteral() {
            return _literal;
        }

        public override string Name => _literal;

        public override void Parse(StringReader reader, CommandContextBuilder<TS> contextBuilder) {
            var start = reader.GetCursor();
            var end = Parse(reader);
            if (end > -1) {
                contextBuilder.WithNode(this, StringRange.Between(start, end));
                return;
            }

            throw CommandSyntaxException.BuiltInExceptions.LiteralIncorrect().CreateWithContext(reader, _literal);
        }

        private int Parse(StringReader reader) {
            var start = reader.GetCursor();
            if (reader.CanRead(_literal.Length)) {
                var end = start + _literal.Length;
                if (reader.GetString().Substring(start, end - start).Equals(_literal)) {
                    reader.SetCursor(end);
                    if (!reader.CanRead() || reader.Peek() == ' ') {
                        return end;
                    } else {
                        reader.SetCursor(start);
                    }
                }
            }
            return -1;
        }

        public override Task<Suggestions> ListSuggestions(CommandContext<TS> context, SuggestionsBuilder builder) {
            if (_literalLowerCase.StartsWith(builder.GetRemainingLowerCase())) {
                return builder.Suggest(_literal).BuildFuture();
            } else {
                return Suggestions.Empty();
            }
        }

        protected override bool IsValidInput(string input) {
            return Parse(new StringReader(input)) > -1;
        }

        public override bool Equals(object o) {
            if (this == o) return true;
            if (!(o is LiteralCommandNode<TS> that)) return false;

            if (!_literal.Equals(that._literal)) return false;
            return Equals(that);
        }

        public override string GetUsageText() {
            return _literal;
        }

        public override int GetHashCode() {
            var result = _literal.GetHashCode();
            result = 31 * result + base.GetHashCode();
            return result;
        }

        public override ArgumentBuilder<TS> CreateBuilder() {
            var builder = LiteralArgumentBuilder<TS>.Literal(this._literal);
            builder.Requires(Requirement);
            builder.Forward(Redirect, RedirectModifier, IsFork);
            if (Command != null) {
                builder.Executes(Command);
            }
            return builder;
        }

        protected override string GetSortedKey() {
            return _literal;
        }

        public override IEnumerable<string> GetExamples() => new [] { _literal };

        public override string ToString() {
            return "<literal " + _literal + ">";
        }
    }
}
