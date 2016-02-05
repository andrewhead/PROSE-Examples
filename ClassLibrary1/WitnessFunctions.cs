using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.ProgramSynthesis.Learning;
using System.Collections.Generic;
using Microsoft.ProgramSynthesis.Rules;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace SubstringExtraction
{
    static class WitnessFunctions
    {
        [WitnessFunction("Substring", 1)]
        static DisjunctiveExamplesSpec WitnessPositionPair(GrammarRule rule, int parameter, ExampleSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();
            foreach (var example in spec.Examples)
            {
                State inputState = example.Key;
                var inp = (string)inputState[rule.Body[0]];
                var substring = (string)example.Value;
                var occurrences = new List<Tuple<int?, int?>>();
                for (int i = inp.IndexOf(substring); i >= 0; i = inp.IndexOf(substring, i + 1))
                {
                    occurrences.Add(Tuple.Create((int?)i, (int?)i + substring.Length));
                }
                if (occurrences.Count == 0) return null;
                result[inputState] = occurrences;
            }
            return new DisjunctiveExamplesSpec(result);
        }

        [WitnessFunction("AbsolutePosition", 1)]
        static DisjunctiveExamplesSpec WitnessK(GrammarRule rule, int parameter, DisjunctiveExamplesSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();
            foreach (var example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var ks = new HashSet<int?>();
                var inp = (string)inputState[rule.Body[0]];
                foreach (int? pos in example.Value)
                {
                    ks.Add(pos);
                    ks.Add(pos - inp.Length - 1);
                }
                if (ks.Count == 0) return null;
                result[inputState] = ks.Cast<object>();
            }
            return new DisjunctiveExamplesSpec(result);
        }

        static Regex[] UsefulRegexes =
        {
            new Regex(@"[A-Za-z]+"),
            // new Regex(@"\d+"),
            new Regex(@"[0-9]+"),
            new Regex(@"$")
        };

        static void BuildStringMatches(String inp, out List<Tuple<Match, Regex>>[] leftMatches, out List<Tuple<Match, Regex>>[] rightMatches)
        {
            leftMatches = new List<Tuple<Match, Regex>>[inp.Length + 1];
            rightMatches = new List<Tuple<Match, Regex>>[inp.Length + 1];
            for (int p = 0; p <= inp.Length; ++p)
            {
                leftMatches[p] = new List<Tuple<Match, Regex>>();
                rightMatches[p] = new List<Tuple<Match, Regex>>();
            }
            foreach (Regex r in UsefulRegexes)
            {
                foreach (Match m in r.Matches(inp))
                {
                    leftMatches[m.Index + m.Length].Add(Tuple.Create(m, r));
                    rightMatches[m.Index].Add(Tuple.Create(m, r));
                }
            }
        }

        [WitnessFunction("RegexPosition", 1)]
        static DisjunctiveExamplesSpec WitnessRegexPair(GrammarRule rule, int parameter,  DisjunctiveExamplesSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();
            foreach (var example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var inp = (string)inputState[rule.Body[0]];
                List<Tuple<Match, Regex>>[] leftMatches, rightMatches;
                BuildStringMatches(inp, out leftMatches, out rightMatches);
                var regexes = new List<Tuple<Regex, Regex>>();
                foreach (int? pos in example.Value)
                {
                    regexes.AddRange(from l in leftMatches[pos.Value]
                                     from r in rightMatches[pos.Value]
                                     select Tuple.Create(l.Item2, r.Item2));
                }
                if (regexes.Count == 0) return null;
                result[inputState] = regexes;
            }
            return new DisjunctiveExamplesSpec(result);
        }

        [WitnessFunction("RegexPosition", 2, DependsOnParameters = new[] { 1 })]
        static DisjunctiveExamplesSpec WitnessKForRegexPair(GrammarRule rule, int parameter,  DisjunctiveExamplesSpec spec, ExampleSpec rrSpec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();
            foreach (var example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var inp = (string)inputState[rule.Body[0]];
                var regexPair = (Tuple<Regex, Regex>)rrSpec.Examples[inputState];
                Regex left = regexPair.Item1;
                Regex right = regexPair.Item2;
                var rightMatches = right.Matches(inp).Cast<Match>().ToDictionary(m => m.Index);
                var matchPositions = new List<int>();
                foreach (Match m in left.Matches(inp))
                {
                    if (rightMatches.ContainsKey(m.Index + m.Length))
                        matchPositions.Add(m.Index + m.Length);
                }
                var ks = new HashSet<int?>();
                foreach (int? pos in example.Value)
                {
                    int occurrence = matchPositions.BinarySearch(pos.Value);
                    if (occurrence < 0) continue;
                    ks.Add(occurrence);
                    ks.Add(occurrence - matchPositions.Count);
                }
                if (ks.Count == 0) return null;
                result[inputState] = ks.Cast<object>();
            }
            return new DisjunctiveExamplesSpec(result);
        }

    }
}
