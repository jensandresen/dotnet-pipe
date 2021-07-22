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
            var result = CommandLineParser.Parse(Array.Empty<string>());
            Assert.Empty(result.Steps);
        }

        [Fact]
        public void returns_expected_variables_when_parsing_empty_list()
        {
            var result = CommandLineParser.Parse(Array.Empty<string>());
            Assert.Empty(result.Variables);
        }

        [Fact]
        public void returns_expected_steps_when_single_is_defined()
        {
            var result = CommandLineParser.Parse(new []{"foo"});
            Assert.Equal(
                new[] {"foo"},
                result.Steps
            );
        }

        [Fact]
        public void returns_expected_steps_when_multiple_is_defined()
        {
            var result = CommandLineParser.Parse(new []{"foo", "bar"});
            Assert.Equal(
                new[] {"foo", "bar"},
                result.Steps
            );
        }

        [Fact]
        public void returns_expected_variables_when_step_is_defined_but_no_variables()
        {
            var result = CommandLineParser.Parse(new []{"dummy"});
            Assert.Empty(result.Variables);
        }

        [Fact]
        public void returns_expected_variables_when_step_is_defined_and_single_variables_is_defined()
        {
            var result = CommandLineParser.Parse(new []{"dummy", "foo=bar"});
            Assert.Equal(
                new[] {KeyValuePair.Create<string, string>("foo", "bar")},
                result.Variables
            );
        }

        [Fact]
        public void returns_expected_step_variable_when_both_is_defined()
        {
            var result = CommandLineParser.Parse(new []
            {
                "foo", 
                "bar", 
                "baz=qux",
                "1=2"
            });
            
            Assert.Equal(
                new[] {"foo", "bar"},
                result.Steps
            );

            Assert.Equal(
                new[]
                {
                    KeyValuePair.Create<string, string>("baz", "qux"),
                    KeyValuePair.Create<string, string>("1", "2"),
                },
                result.Variables
            );
        }

        [Fact]
        public void returns_expected_filePath_when_none_is_defined()
        {
            var result = CommandLineParser.Parse(new []{"dummy1", "dummy2=dummy3"});
            Assert.Null(result.FilePath);
        }

        [Fact]
        public void returns_expected_filePath_when_single_is_defined()
        {
            var result = CommandLineParser.Parse(new []{"dummy", "-f", "foo"});
            Assert.Equal("foo", result.FilePath);
        }

        [Fact]
        public void returns_expected_filePath_when_multiple_files_is_defined()
        {
            var result = CommandLineParser.Parse(new []{"dummy", "-f", "foo", "-f", "bar"});
            Assert.Equal("bar", result.FilePath);
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
            var result = CommandLineParser.Parse(new []
            {
                "foo", 
                "-f", 
                "bar", 
                "baz=qux",
            });
            
            Assert.Equal(
                expected: new[] {"foo"},
                actual: result.Steps
            );

            Assert.Equal(
                expected: "bar",
                actual: result.FilePath
            );
            
            Assert.Equal(
                expected: new[]
                {
                    KeyValuePair.Create<string, string>("baz", "qux"),
                },
                actual: result.Variables
            );
        }
        
        [Fact]
        public void returns_expected_verbosity_when_not_set()
        {
            var result = CommandLineParser.Parse(new []{"dummy"});
            Assert.False(result.IsVerbose);
        }

        [Fact]
        public void returns_expected_verbosity_when_set()
        {
            var result = CommandLineParser.Parse(new []{"dummy", "-v"});
            Assert.True(result.IsVerbose);
        }

        [Fact]
        public void duno()
        {
            var result = CommandLineParser.Parse(new []{"build", "test", "-v", "-f", "Makefile"});

            Assert.Equal(
                expected: new[] {"build", "test"},
                actual: result.Steps
            );
            
            Assert.True(result.IsVerbose);
            Assert.Equal(
                expected: "Makefile",
                actual: result.FilePath
            );
            
            Assert.Empty(result.Variables);
        }

        [Fact]
        public void duno2()
        {
            Assert.Throws<InvalidArgumentException>(() => CommandLineParser.Parse(new[] {"dummy", "-x"}));
        }
    }
}