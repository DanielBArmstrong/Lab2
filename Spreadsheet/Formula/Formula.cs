// Skeleton written by Joe Zachary for CS 3500, January 2014

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SpreadsheetUtilities
{
    /// <summary>
    /// Represents formulas written in standard infix notation using standard precedence
    /// rules.  Provides a means to evaluate Formulas.  Formulas can be composed of
    /// non-negative floating-point numbers, variables, left and right parentheses, and
    /// the four binary operator symbols +, -, *, and /.  (The unary operators + and -
    /// are not allowed.)
    /// </summary>
    public class Formula
    {
        private List<string> tokens;
        
        /// <summary>
        /// Creates a Formula from a string that consists of a standard infix expression composed
        /// from non-negative floating-point numbers (using standard C# syntax for double/int literals), 
        /// variable symbols (one or more letters followed by one or more digits), left and right
        /// parentheses, and the four binary operator symbols +, -, *, and /.  White space is
        /// permitted between tokens, but is not required.
        /// 
        /// An example of a valid parameter to this constructor is "2.5e9 + x5 / 17".
        /// Examples of invalid parameters are "x", "-5.3", and "2 5 + 3";
        /// 
        /// If the formula is syntacticaly invalid, throws a FormulaFormatException with an 
        /// explanatory Message.
        /// </summary>
        public Formula(String formula)
        {
            tokens = GetTokens(formula).ToList<string>();
            if (tokens.Capacity < 0)
                throw new FormulaFormatException(formula);

            foreach (String token in tokens)
            {
                double num1;
                if (double.TryParse(token, out num1))
                {
                    if (num1 < 0)
                        throw new FormulaFormatException(formula);
                    continue;
                }

                if (!token.Equals("/") || !token.Equals("*") || !token.Equals("+") || !token.Equals("-")
                    || !token.Equals("(") || !token.Equals(")") || !Regex.IsMatch(token, @"[a-zA-Z]+\d+"))
                    throw new FormulaFormatException(formula);

            }

            int leftParen = 0;
            int rightParen = 0;
            bool check1 = false;
            bool check2 = false;
            bool check3 = false;
            foreach (String token in tokens)
            {
                double num2;
                if (token.Equals("("))
                    leftParen++;
                if (token.Equals(")"))
                    rightParen++;
                if (rightParen > leftParen)
                    throw new FormulaFormatException(formula);
               
                if (check1)
                {
                    if (!token.Equals("(") || !double.TryParse(token, out num2) || !Regex.IsMatch(token, @"[a-zA-Z]+\d+"))
                        throw new FormulaFormatException(formula);
                    check1 = false;
                }

                if (check2)
                {
                    if (!token.Equals(")") || !token.Equals("/") || !token.Equals("*") || !token.Equals("+") || !token.Equals("-"))
                        throw new FormulaFormatException(formula);
                    check2 = false;
                }

                if(check3)
                {
                    if (token.Equals("0"))
                        throw new FormulaFormatException(formula);
                    check3 = false;
                }

                if (token.Equals("(") || token.Equals("/") || token.Equals("*") || token.Equals("+") || token.Equals("-"))
                    check1 = true;

                double num3;
                if (token.Equals(")") || Regex.IsMatch(token, @"[a-zA-Z]+\d+") || double.TryParse(token, out num3))
                    check2 = true;
                if (token.Equals("/"))
                    check3 = true;
            }

                if (leftParen != rightParen)
                    throw new FormulaFormatException(formula);

                double num4;
                if (!tokens.First().Equals("(") || !double.TryParse(tokens.First(), out num4) || !Regex.IsMatch(tokens.First(), @"[a-zA-Z]+\d+"))
                    throw new FormulaFormatException(formula);
                if (!tokens.Last().Equals(")") || !double.TryParse(tokens.First(), out num4) || !Regex.IsMatch(tokens.First(), @"[a-zA-Z]+\d+"))
                    throw new FormulaFormatException(formula);
   
        }

        /// <summary>
        /// A Lookup function is one that maps some strings to double values.  Given a string,
        /// such a function can either return a double (meaning that the string maps to the
        /// double) or throw an IllegalArgumentException (meaning that the string is unmapped.
        /// Exactly how a Lookup function decides which strings map to doubles and which
        /// don't is up to the implementation of that function.
        /// </summary>
        public delegate double Lookup(string s);

        /// <summary>
        /// Evaluates this Formula, using lookup to determine the values of variables.  
        /// 
        /// If no undefined variables or divisions by zero are encountered when evaluating 
        /// this Formula, the value is returned.  Otherwise, throw a FormulaEvaluationException  
        /// with an explanatory Message.
        /// </summary>
        public double Evaluate(Lookup lookup)
        {
            Stack<double> value = new Stack<double>();
            Stack<string> operate = new Stack<string>();

            foreach(string token in tokens)
            {
                double num;
                if(double.TryParse(token, out num))
                {
                    if(operate.Peek().Equals("*"))
                    {
                        double val = value.Pop();
                        string op = operate.Pop();
                        double temp = num * val;
                        value.Push(temp);
                    }
                    
                    if(operate.Peek().Equals("/"))
                    {
                        double val = value.Pop();
                        string op = operate.Pop();
                        double temp = num / val;
                        value.Push(temp);
                    }
                
                if(Regex.IsMatch(tokens.First(), @"[a-zA-Z]+\d+"))
                {
                    double tok = lookup(token);

                }
                
                }

            }

            return 0;
        }

        /// <summary>
        /// Given a formula, enumerates the tokens that compose it.  Tokens are left paren,
        /// right paren, one of the four operator symbols, a string consisting of one or more
        /// letters followed by one or more digits, a double literal, and anything that doesn't
        /// match one of those patterns.  There are no empty tokens, and no token contains white space.
        /// </summary>
        private static IEnumerable<string> GetTokens(String formula)
        {
            // Patterns for individual tokens
            String lpPattern = @"\(";
            String rpPattern = @"\)";
            String opPattern = @"[\+\-*/]";
            String varPattern = @"[a-zA-Z]+\d+";
            String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: e[\+-]?\d+)?";
            String spacePattern = @"\s+";

            // Overall pattern
            String pattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                            lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

            // Enumerate matching tokens that don't consist solely of white space.
            foreach (String s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
            {
                if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
                {
                    yield return s;
                }
            }
        }
    }

    /// <summary>
    /// Used to report syntactic errors in the argument to the Formula constructor.
    /// </summary>
    public class FormulaFormatException : Exception
    {
        /// <summary>
        /// Constructs a FormulaFormatException containing the explanatory message.
        /// </summary>
        public FormulaFormatException(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Used to report errors that occur when evaluating a Formula.
    /// </summary>
    public class FormulaEvaluationException : Exception
    {
        /// <summary>
        /// Constructs a FormulaEvaluationException containing the explanatory message.
        /// </summary>
        public FormulaEvaluationException(String message)
            : base(message)
        {
        }
    }
}
