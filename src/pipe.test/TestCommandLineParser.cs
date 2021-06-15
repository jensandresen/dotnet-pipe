using System;
using System.Collections.Generic;
using pipe.Exceptions;
using Xunit;

namespace pipe.test
{
    public class TestCommandLineParser
    {
        [Fact]
        public void returns_expected_steps_when_parsing_empty_list()
        {
            var (steps, _, _) = CommandLineParser.Parse(Array.Empty<string>());
            Assert.Empty(steps);
        }

        [Fact]
        public void returns_expected_variables_when_parsing_empty_list()
        {
            var (_, variables, _) = CommandLineParser.Parse(Array.Empty<string>());
            Assert.Empty(variables);
        }

        [Fact]
        public void returns_expected_steps_when_single_is_defined()
        {
            var (steps, _, _) = CommandLineParser.Parse(new []{"foo"});
            Assert.Equal(
                new[] {"foo"},
                steps
            );
        }

        [Fact]
        public void returns_expected_steps_when_multiple_is_defined()
        {
            var (steps, _, _) = CommandLineParser.Parse(new []{"foo", "bar"});
            Assert.Equal(
                new[] {"foo", "bar"},
                steps
            );
        }

        [Fact]
        public void returns_expected_variables_when_step_is_defined_but_no_variables()
        {
            var (_, variables, _) = CommandLineParser.Parse(new []{"dummy"});
            Assert.Empty(variables);
        }

        [Fact]
        public void returns_expected_variables_when_step_is_defined_and_single_variables_is_defined()
        {
            var (_, variables, _) = CommandLineParser.Parse(new []{"dummy", "foo=bar"});
            Assert.Equal(
                new[] {KeyValuePair.Create<string, string>("foo", "bar")},
                variables
            );
        }

        [Fact]
        public void returns_expected_step_variable_when_both_is_defined()
        {
            var (steps, variables, _) = CommandLineParser.Parse(new []
            {
                "foo", 
                "bar", 
                "baz=qux",
                "1=2"
            });
            
            Assert.Equal(
                new[] {"foo", "bar"},
                steps
            );

            Assert.Equal(
                new[]
                {
                    KeyValuePair.Create<string, string>("baz", "qux"),
                    KeyValuePair.Create<string, string>("1", "2"),
                },
                variables
            );
        }

        [Fact]
        public void returns_expected_filePath_when_none_is_defined()
        {
            var (_, _, filePath) = CommandLineParser.Parse(new []{"dummy1", "dummy2=dummy3"});
            Assert.Null(filePath);
        }

        [Fact]
        public void returns_expected_filePath_when_single_is_defined()
        {
            var (_, _, filePath) = CommandLineParser.Parse(new []{"dummy", "-f", "foo"});
            Assert.Equal("foo", filePath);
        }

        [Fact]
        public void returns_expected_filePath_when_multiple_files_is_defined()
        {
            var (_, _, filePath) = CommandLineParser.Parse(new []{"dummy", "-f", "foo", "-f", "bar"});
            Assert.Equal("bar", filePath);
        }

        [Fact]
        public void throws_expected_exception_if_filePath_flag_is_defined_but_missing_path()
        {
            Assert.Throws<CommandLineParsingException>(() => CommandLineParser.Parse(new[] {"dummy", "-f"}));
        }

        [Fact]
        public void throws_expected_exception_if_filePath_is_a_variable_ovveride()
        {
            Assert.Throws<CommandLineParsingException>(() => CommandLineParser.Parse(new[] {"dummy", "-f", "foo=bar"}));
        }

        [Fact]
        public void returns_expected_step_variable_and_filepath_when_all_is_defined()
        {
            var (steps, variables, filePath) = CommandLineParser.Parse(new []
            {
                "foo", 
                "-f", 
                "bar", 
                "baz=qux",
            });
            
            Assert.Equal(
                expected: new[] {"foo"},
                actual: steps
            );

            Assert.Equal(
                expected: "bar",
                actual: filePath
            );
            
            Assert.Equal(
                expected: new[]
                {
                    KeyValuePair.Create<string, string>("baz", "qux"),
                },
                actual: variables
            );
        }
    }
}