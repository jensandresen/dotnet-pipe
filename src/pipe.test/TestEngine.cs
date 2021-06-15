using System.IO;
using System.Linq;
using pipe.Exceptions;
using pipe.Shells;
using pipe.test.Builders;
using pipe.test.TestDoubles;
using Xunit;
using Xunit.Abstractions;

namespace pipe.test
{
    public class TestEngine
    {
        [Fact]
        public void throws_exception_if_variables_has_been_defined_as_args_but_no_step()
        {
            var sut = new EngineBuilder().Build();
            Assert.Throws<MissingRequiredStepException>(() => sut.Run(new[] {"dummy=dummy"}));
        }

        [Fact]
        public void throws_exception_when_unable_to_locate_a_pipeline_file()
        {
            var sut = new EngineBuilder().Build();
            Assert.Throws<FileNotFoundException>(() => sut.Run(new[] {"dummy"}));
        }

        [Fact]
        public void throws_expected_exception_if_requested_step_is_not_defined_in_pipeline_file()
        {
            var sut = new EngineBuilder()
                .WithFileSystem(new StubFileSystem(
                    fileExists: true,
                    fileContents: new[] {""})
                )
                .Build();
            
            Assert.Throws<StepNotDefinedException>(() => sut.Run(new[] {"foo"}));
        }

        [Fact]
        public void runs_expected_step_from_pipeline_file_when_requested_from_args()
        {
            var spy = new SpyCommandLineExecutor();
            
            var sut = new EngineBuilder()
                .WithCommandLineExecutor(spy)
                .WithFileSystem(new StubFileSystem(
                    fileExists: true,
                    fileContents: new[] {"foo:", "  bar", "baz:", "  qux"})
                )
                .Build();

            sut.Run(new[] {"baz"});

            Assert.Equal(new[] {"qux"}, spy.executedArguments);
        }

        [Fact]
        public void runs_expected_actions_when_step_contains_multiple()
        {
            var spy = new SpyCommandLineExecutor();
            
            var sut = new EngineBuilder()
                .WithCommandLineExecutor(spy)
                .WithFileSystem(new StubFileSystem(
                    fileExists: true,
                    fileContents: new[] {"foo:", "  bar", "  baz", "  qux"})
                )
                .Build();

            sut.Run(new[] {"foo"});

            Assert.Equal(new[] {"bar", "baz", "qux"}, spy.executedArguments);
        }

        [Fact]
        public void default_to_running_the_first_step_in_pipeline_file_if_none_has_been_specified()
        {
            var spy = new SpyCommandLineExecutor();
            
            var sut = new EngineBuilder()
                .WithCommandLineExecutor(spy)
                .WithFileSystem(new StubFileSystem(
                    fileExists: true,
                    fileContents: new[] {"foo:", "  bar", "baz:", "  qux"})
                )
                .Build();

            sut.Run(new string[] {});

            Assert.Equal(new[] {"bar"}, spy.executedArguments);
        }

        [Fact]
        public void uses_shell_from_variables_in_pipeline_file()
        {
            var spy = new SpyCommandFactory(new StubCommandFactory(new Command("foo", _ => "dummy")));
            var sut = new EngineBuilder()
                .WithCommandFactory(spy)
                .WithFileSystem(new StubFileSystem(
                    fileExists: true,
                    fileContents: new[]
                    {
                        "SHELL=foo", 
                        "dummy:", 
                        "  dummy"
                    })
                )
                .Build();

            sut.Run(new string[] {});

            Assert.Equal("foo", spy.wasCreatedWith);
        }

        [Theory]
        [InlineData(OperatingSystemType.Windows, "powershell")]
        [InlineData(OperatingSystemType.Linux, "sh")]
        [InlineData(OperatingSystemType.Mac, "bash")]
        public void if_shell_is_not_defined_as_variable_the_default_shell_for_a_specific_OS_is_used(OperatingSystemType os, string expectedShell)
        {
            var spy = new SpyCommandLineExecutor();

            var commandFactory = new RealCommandFactoryBuilder()
                .WithOperatingSystemTypeProvider(new StubOperatingSystemTypeProvider(os))
                .Build();
            
            var sut = new EngineBuilder()
                .WithCommandFactory(commandFactory)
                .WithCommandLineExecutor(spy)
                .WithFileSystem(new StubFileSystem(
                    fileExists: true,
                    fileContents: new[] {"dummy:", "  dummy"}
                ))
                .Build();
            
            sut.Run(new string[] {});

            Assert.Equal(expectedShell, spy.executedShell);
        }

        [Fact]
        public void if_an_action_fails_additional_step_actions_are_not_executed()
        {
            var spy = new SpyCommandLineExecutor();

            var sut = new EngineBuilder()
                .WithCommandLineExecutor(new ErroneusCommandLineExecutorDecorator(spy, invocationToFailOn: 2))
                .WithFileSystem(new StubFileSystem(
                    fileExists: true,
                    fileContents: new[] {"foo:", "  bar", "  baz", "  qux"})
                )
                .Build();

            try
            {
                sut.Run(new[] {"foo"});
            }
            catch
            {
                // ignored
            }

            Assert.Equal(new[] {"bar"}, spy.executedArguments);
        }
        
        [Fact]
        public void if_one_step_fails_additional_steps_are_not_executed()
        {
            var spy = new SpyCommandLineExecutor();

            var sut = new EngineBuilder()
                .WithCommandLineExecutor(new ErroneusCommandLineExecutorDecorator(spy, invocationToFailOn: 2))
                .WithFileSystem(new StubFileSystem(
                    fileExists: true,
                    fileContents: new[] {"foo:", "  bar", "baz:", "  qux"})
                )
                .Build();

            try
            {
                sut.Run(new[] {"foo", "baz"});
            }
            catch
            {
                // ignored
            }

            Assert.Equal(new[] {"bar"}, spy.executedArguments);
        }
        
        [Fact]
        public void a_step_can_have_a_pre_step_that_are_executed_before_it_self()
        {
            var spy = new SpyCommandLineExecutor();

            var sut = new EngineBuilder()
                .WithCommandLineExecutor(spy)
                .WithFileSystem(new StubFileSystem(
                    fileExists: true,
                    fileContents: new[] {"foo: baz", "  bar", "baz:", "  qux"})
                )
                .Build();

            sut.Run(new[] {"foo"});
            
            Assert.Equal(new[] {"qux", "bar"}, spy.executedArguments);
        }
        
        [Fact]
        public void a_step_can_have_multiple_pre_steps_that_are_executed_before_it_self()
        {
            var spy = new SpyCommandLineExecutor();

            var sut = new EngineBuilder()
                .WithCommandLineExecutor(spy)
                .WithFileSystem(new StubFileSystem(
                    fileExists: true,
                    fileContents: new[] {"foo: bar baz qux", "  foo-action", "bar:", "  bar-action", "baz:", "  baz-action", "qux:", "  qux-action"})
                )
                .Build();

            sut.Run(new[] {"foo"});
            
            Assert.Equal(new[] {"bar-action", "baz-action", "qux-action", "foo-action"}, spy.executedArguments);
        }
        
        [Fact]
        public void a_step_is_not_required_to_have_an_action_if_it_has_a_pre_step()
        {
            var spy = new SpyCommandLineExecutor();

            var sut = new EngineBuilder()
                .WithCommandLineExecutor(spy)
                .WithFileSystem(new StubFileSystem(
                    fileExists: true,
                    fileContents: new[] {"foo: bar", "bar:", "  bar-action"})
                )
                .Build();

            sut.Run(new[] {"foo"});
            
            Assert.Equal(new[] {"bar-action"}, spy.executedArguments);
        }
        
        [Fact]
        public void supports_nested_steps_aka_pre_steps_of_pre_steps()
        {
            var spy = new SpyCommandLineExecutor();

            var sut = new EngineBuilder()
                .WithCommandLineExecutor(spy)
                .WithFileSystem(new StubFileSystem(
                    fileExists: true,
                    fileContents: new[] {"foo: bar","  foo-action", "bar: baz", "  bar-action", "baz: qux", "  baz-action", "qux:", "  qux-action"})
                )
                .Build();

            sut.Run(new[] {"foo"});
            
            Assert.Equal(new[] {"qux-action", "baz-action", "bar-action", "foo-action"}, spy.executedArguments);
        }
        
        [Fact]
        public void a_step_cannot_reference_it_self_as_a_pre_step()
        {
            var spy = new SpyCommandLineExecutor();

            var sut = new EngineBuilder()
                .WithCommandLineExecutor(spy)
                .WithFileSystem(new StubFileSystem(
                    fileExists: true,
                    fileContents: new[] {"foo: foo","  dummy"})
                )
                .Build();

            Assert.Throws<StepSelfReferencesException>(() => sut.Run(new[] {"foo"}));
        }
        
        [Fact]
        public void prevent_circular_execution_with_steps_that_has_pre_steps()
        {
            var spy = new SpyCommandLineExecutor();

            var sut = new EngineBuilder()
                .WithCommandLineExecutor(spy)
                .WithFileSystem(new StubFileSystem(
                    fileExists: true,
                    fileContents: new[] {"foo: bar","  dummy", "bar: foo", "  dummy"})
                )
                .Build();

            Assert.Throws<StepCircularReferenceException>(() => sut.Run(new[] {"foo"}));
        }
        
        [Fact]
        public void if_a_pre_step_fails_its_parent_step_is_not_executed()
        {
            var spy = new SpyCommandLineExecutor();

            var sut = new EngineBuilder()
                .WithCommandLineExecutor(new ErroneusCommandLineExecutorDecorator(spy, invocationToFailOn: 1))
                .WithFileSystem(new StubFileSystem(
                    fileExists: true,
                    fileContents: new[] {"foo: bar", "  foo-action", "bar:", "  bar-action"})
                )
                .Build();

            try
            {
                sut.Run(new[] {"foo"});
            }
            catch
            {
                // ignored
            }

            Assert.Equal(Enumerable.Empty<string>(), spy.executedArguments);
        }

        [Fact]
        public void variables_are_expanded_when_actions_are_executed()
        {
            var spy = new SpyCommandLineExecutor();

            var variableHelper = new VariableHelperBuilder().Build();
            
            var sut = new EngineBuilder()
                .WithCommandLineExecutor(spy)
                .WithVariableHelper(variableHelper)
                .WithFileSystem(new StubFileSystem(
                    fileExists: true,
                    fileContents: new[]
                    {
                        "FOO=bar",
                        "foo:",
                        "  foo-$(FOO)"
                    })
                )
                .Build();

            sut.Run(new[] {"foo"});
            
            Assert.Equal(new[]{"foo-bar"}, spy.executedArguments);
        }
        
        // test: environment variables are expanded when actions are executed
    }
}