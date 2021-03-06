namespace KaLib.Brigadier.Exceptions
{
    public class Dynamic3CommandExceptionType : ICommandExceptionType {
        private readonly Function _function;

        public Dynamic3CommandExceptionType(Function function) {
            this._function = function;
        }

        public CommandSyntaxException Create(object a, object b, object c) {
            return new CommandSyntaxException(this, _function(a, b, c));
        }

        public CommandSyntaxException CreateWithContext(IMmutableStringReader reader, object a, object b, object c) {
            return new CommandSyntaxException(this, _function(a, b, c), reader.GetString(), reader.GetCursor());
        }

        public delegate IMessage Function(object a, object b, object c);
    }
}
