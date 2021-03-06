using KaLib.Brigadier.Tree;

namespace KaLib.Brigadier.Builder
{
    public class LiteralArgumentBuilder<TS> : ArgumentBuilder<TS, LiteralArgumentBuilder<TS>> {
        private readonly string _literal;

        protected LiteralArgumentBuilder(string literal) {
            this._literal = literal;
        }

        public static LiteralArgumentBuilder<TS> Literal(string name) {
            return new LiteralArgumentBuilder<TS>(name);
        }

        protected override LiteralArgumentBuilder<TS> GetThis() {
            return this;
        }

        public string GetLiteral() => _literal;

        public override CommandNode<TS> Build() {
            var result = new LiteralCommandNode<TS>(GetLiteral(), Command, Requirement, GetRedirect(), RedirectModifier, IsFork);

            foreach (var argument in GetArguments()) {
                result.AddChild(argument);
            }

            return result;
        }
    }
}
