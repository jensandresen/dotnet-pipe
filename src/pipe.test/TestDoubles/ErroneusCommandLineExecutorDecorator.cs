using System;
using pipe.Shells;

namespace pipe.test.TestDoubles
{
    public class ErroneusCommandLineExecutorDecorator : ICommandLineExecutor
    {
        private readonly ICommandLineExecutor _inner;
        private readonly int _invocationToFailOn;
        private int _invocation = 0;

        public ErroneusCommandLineExecutorDecorator(ICommandLineExecutor inner, int invocationToFailOn = 1)
        {
            _inner = inner;
            _invocationToFailOn = invocationToFailOn;
        }

        public void Execute(string shell, string arguments)
        {
            _invocation++;

            if (_invocation == _invocationToFailOn)
            {
                throw new Exception($"Failing - on purpose - on invocation {_invocation} of {nameof(Execute)}(...) in {this.GetType().FullName}.");
            }
            
            _inner.Execute(shell, arguments);
        }
    }
}