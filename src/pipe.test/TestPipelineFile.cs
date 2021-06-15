using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Xunit;

namespace pipe.test
{
    public class TestPipelineFile
    {
        [Theory]
        [InlineData("foo")]
        [InlineData("bar")]
        public void parse_simple_step_returns_expected_name_of_step(string expected)
        {
            var sut = PipelineFile.Parse(new[]
            {
                $"{expected}:",
                "   dummy",
            });

            Assert.Equal(
                expected: new[] {expected},
                actual: sut.Steps.Select(x => x.Name)
            );
        }

        [Theory]
        [InlineData("foo")]
        [InlineData("bar")]
        public void parse_simple_step_with_single_action_returns_expected_action(string expected)
        {
            var sut = PipelineFile.Parse(new[]
            {
                $"dummy:",
                $"   {expected}",
            });

            var result = Assert.Single(sut.Steps);

            Assert.Equal(
                expected: new[] {expected},
                actual: result.Actions
            );
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("  ")]
        [InlineData("   ")]
        [InlineData("\t")]
        public void can_handle_empty_line_after_step_with_single_actions(string emptyLine)
        {
            var sut = PipelineFile.Parse(new[]
            {
                "dummy:",
                "   foo",
                emptyLine,
            });

            var result = Assert.Single(sut.Steps);

            Assert.Equal(
                expected: new [] {"foo"},
                actual: result.Actions
            );
        }

        [Fact]
        public void parse_simple_step_with_multiple_actions_returns_expected_actions()
        {
            var sut = PipelineFile.Parse(new[]
            {
                "dummy:",
                "   foo",
                "   bar",
            });

            var result = Assert.Single(sut.Steps);

            Assert.Equal(
                expected: new [] {"foo", "bar"},
                actual: result.Actions
            );
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("  ")]
        [InlineData("   ")]
        [InlineData("\t")]
        public void can_handle_empty_line_after_step_with_multiple_actions(string emptyLine)
        {
            var sut = PipelineFile.Parse(new[]
            {
                "dummy:",
                "   foo",
                "   bar",
                emptyLine,
            });

            var result = Assert.Single(sut.Steps);

            Assert.Equal(
                expected: new [] {"foo", "bar"},
                actual: result.Actions
            );
        }
        
        [Fact]
        public void parse_multiple_simple_steps_with_single_action_returns_expected()
        {
            var sut = PipelineFile.Parse(new[]
            {
                "foo:",
                "   bar",
                "",
                "baz:",
                "   qux",
            });

            Assert.Equal(
                expected: new[]
                {
                    new Step("foo", new[] {"bar"}),
                    new Step("baz", new[] {"qux"})
                },
                actual: sut.Steps
            );
        }

        [Fact]
        public void parse_multiple_simple_steps_with_multiple_actions_returns_expected()
        {
            var sut = PipelineFile.Parse(new[]
            {
                "foo:",
                "   bar1",
                "   bar2",
                "",
                "baz:",
                "   qux1",
                "   qux2",
            });

            Assert.Equal(
                expected: new[]
                {
                    new Step("foo", new[] {"bar1", "bar2"}),
                    new Step("baz", new[] {"qux1", "qux2"})
                },
                actual: sut.Steps
            );
        }

        [Fact]
        public void can_handle_comments()
        {
            var sut = PipelineFile.Parse(new[]
            {
                "# first comment",
                "foo:",
                "   bar",
                "",
                "  # second comment",
                "baz:",
                "   qux",
            });

            Assert.Equal(
                expected: new[]
                {
                    new Step("foo", new[] {"bar"}),
                    new Step("baz", new[] {"qux"})
                },
                actual: sut.Steps
            );
        }

        [Fact]
        public void can_handle_multi_line_action()
        {
            var sut = PipelineFile.Parse(new[]
            {
                "foo:",
                @"   bar \",
                @"   baz \",
                @"   qux",
            });

            Assert.Equal(
                expected: new[]
                {
                    new Step("foo", new[] {"bar baz qux"}),
                },
                actual: sut.Steps
            );
        }

        [Fact]
        public void can_handle_mix_of_single_and_multi_line_actions()
        {
            var sut = PipelineFile.Parse(new[]
            {
                "foo:",
                @"   bar",
                @"   baz1 \",
                @"   baz2",
                @"   qux",
            });

            Assert.Equal(
                expected: new[]
                {
                    new Step("foo", new[] {"bar", "baz1 baz2", "qux"}),
                },
                actual: sut.Steps
            );
        }

        [Fact]
        public void returns_expected_variables_when_none_has_been_defined()
        {
            var sut = PipelineFile.Parse(new[]
            {
                "dummy:",
                "   dummy",
            });
            
            Assert.Empty(sut.Variables);
        }

        [Fact]
        public void returns_expected_variables_when_single_has_been_defined()
        {
            var sut = PipelineFile.Parse(new[]
            {
                "foo=bar",
                "dummy:",
                "   dummy",
            });

            Assert.Equal(
                expected: new Dictionary<string, string> {{"foo", "bar"}},
                actual: sut.Variables
            );
        }

        [Fact]
        public void returns_expected_variables_when_multiple_has_been_defined()
        {
            var sut = PipelineFile.Parse(new[]
            {
                "foo=bar",
                "baz=qux",
                "dummy:",
                "   dummy",
            });

            Assert.Equal(
                expected: new Dictionary<string, string> {{"foo", "bar"}, {"baz", "qux"}},
                actual: sut.Variables
            );
        }

        [Fact]
        public void variables_can_have_trailing_comments()
        {
            var sut = PipelineFile.Parse(new[]
            {
                "foo=bar # this is a trailing comment!",
                "dummy:",
                "   dummy",
            });

            Assert.Equal(
                expected: new Dictionary<string, string> {{"foo", "bar"}},
                actual: sut.Variables
            );
        }
        
        [Theory]
        [InlineData("foo=bar", "foo", "bar")]
        [InlineData("foo= bar", "foo", "bar")]
        [InlineData("foo =bar", "foo", "bar")]
        [InlineData("foo = bar", "foo", "bar")]
        public void variables_can_have_spaces(string input, string expectedKey, string expectedValue)
        {
            var sut = PipelineFile.Parse(new[]
            {
                input,
                "dummy:",
                "   dummy",
            });

            Assert.Equal(
                expected: new Dictionary<string, string> {{expectedKey, expectedValue}},
                actual: sut.Variables
            );
        }

        [Theory]
        [InlineData("foo")]
        [InlineData("foo-bar")]
        [InlineData("foo_bar")]
        [InlineData("FOO")]
        public void valid_variable_keys(string validValue)
        {
            var sut = PipelineFile.Parse(new[]
            {
                $"{validValue}=foovalue",
                "dummy:",
                "   dummy",
            });

            Assert.Equal(
                expected: new Dictionary<string, string> {{validValue, "foovalue"}},
                actual: sut.Variables
            );
        }

        [Theory]
        [InlineData("foo")]
        [InlineData("foo-bar")]
        [InlineData("foo_bar")]
        public void valid_variable_values(string validValue)
        {
            var sut = PipelineFile.Parse(new[]
            {
                $"foo={validValue}",
                "dummy:",
                "   dummy",
            });

            Assert.Equal(
                expected: new Dictionary<string, string> {{"foo", validValue}},
                actual: sut.Variables
            );
        }

        [Fact]
        public void variable_values_can_be_quoted()
        {
            var sut = PipelineFile.Parse(new[]
            {
                $"foo=\"bar\"",
                "dummy:",
                "   dummy",
            });

            Assert.Equal(
                expected: new Dictionary<string, string> {{"foo", "bar"}},
                actual: sut.Variables
            );
        }
        
        [Fact]
        public void steps_can_also_have_a_pre_step()
        {
            var sut = PipelineFile.Parse(new[]
            {
                $"foo: bar",
                "   dummy",
            });

            Assert.Equal(
                expected: new[] {"bar"},
                actual: sut.Steps.Single().PreStepNames
            );
        }

        [Fact]
        public void steps_can_also_have_multiple_pre_steps()
        {
            var sut = PipelineFile.Parse(new[]
            {
                $"foo: bar baz qux",
                "   dummy",
            });

            Assert.Equal(
                expected: new[] {"bar", "baz", "qux"},
                actual: sut.Steps.Single().PreStepNames
            );
        }
        
        [Fact]
        public void steps_with_pre_step_can_have_actions()
        {
            var sut = PipelineFile.Parse(new[]
            {
                $"foo: dummy",
                "   bar",
            });

            Assert.Equal(
                expected: new[] {"bar"},
                actual: sut.Steps.Single().Actions
            );
        }

        [Fact]
        public void steps_with_pre_step_is_NOT_required_to_have_actions()
        {
            var sut = PipelineFile.Parse(new[]
            {
                $"foo: bar",
            });

            var step = sut.Steps.Single();

            Assert.Equal(
                expected: "foo",
                actual: step.Name
            );
            
            Assert.Equal(
                expected: new[] {"bar"},
                actual: step.PreStepNames
            );
        }
    }
}
