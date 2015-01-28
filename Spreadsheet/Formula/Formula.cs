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
           //if there are no tokens in the string throw format exception.
            if (tokens.Capacity < 0)
                throw new FormulaFormatException("The formula " + formula + " is syntactically invalid");
            //loop through all tokens.
            foreach (String token in tokens)
            {   //if token is a non-negative double it is valid.
                double num1;
                if (double.TryParse(token, out num1))
                {
                    if (num1 < 0)
                        throw new FormulaFormatException("The formula " + formula + " is syntactically invalid");
                    continue;
                }

                //if token does not equal one of the following it is invalid. Throw an exception.
                if (!token.Equals("/") && !token.Equals("*") && !token.Equals("+") && !token.Equals("-")
                    && !token.Equals("(") && !token.Equals(")") && !Regex.IsMatch(token, @"[a-zA-Z]+\d+"))
                    throw new FormulaFormatException("The formula " + formula + " is syntactically invalid");

            }
            
            //set up counters.
            int leftParen = 0;
            int rightParen = 0;
            bool check1 = false;
            bool check2 = false;
            bool check3 = false;
            foreach (String token in tokens)
            {   
                double num2;
                //continue to count each parenthesis. If opens out number closeds throw exception.
                if (token.Equals("("))
                    leftParen++;
                if (token.Equals(")"))
                    rightParen++;
                if (rightParen > leftParen)
                    throw new FormulaFormatException("The formula " + formula + " is syntactically invalid");
               
                //checks that make sure tokens following a specific type of token are valid types.
                if (check1)
                {
                    if (!token.Equals("(") && !double.TryParse(token, out num2) && !Regex.IsMatch(token, @"[a-zA-Z]+\d+"))
                        throw new FormulaFormatException("The formula " + formula + " is syntactically invalid");
                    check1 = false;
                }

                if (check2)
                {
                    if (!token.Equals(")") && !token.Equals("/") && !token.Equals("*") && !token.Equals("+") && !token.Equals("-"))
                        throw new FormulaFormatException("The formula " + formula + " is syntactically invalid");
                    check2 = false;
                }

                if(check3)
                {
                    if (token.Equals("0"))
                        throw new FormulaFormatException("The formula " + formula + " is syntactically invalid");
                    check3 = false;
                }

                //set the checks for the next token depending on the current token.
                if (token.Equals("(") || token.Equals("/") || token.Equals("*") || token.Equals("+") || token.Equals("-"))
                    check1 = true;

                double num3;
                if (token.Equals(")") || Regex.IsMatch(token, @"[a-zA-Z]+\d+") || double.TryParse(token, out num3))
                    check2 = true;
               
                if (token.Equals("/"))
                    check3 = true;
            }
                //number of open and closed parenthesis should be equal.
                if (leftParen != rightParen)
                    throw new FormulaFormatException("The formula " + formula + " is syntactically invalid");

                //checking first and last token to make sure they are of valid type.
                double num4;
                if (!tokens.First().Equals("(") && !double.TryParse(tokens.First(), out num4) && !Regex.IsMatch(tokens.First(), @"[a-zA-Z]+\d+"))
                    throw new FormulaFormatException("The formula " + formula + " is syntactically invalid");
                if (!tokens.Last().Equals(")") && !double.TryParse(tokens.Last(), out num4) && !Regex.IsMatch(tokens.Last(), @"[a-zA-Z]+\d+"))
                    throw new FormulaFormatException("The formula " + formula + " is syntactically invalid");
   
        }

        /// <summary>
        /// A Lookup function is one that maps some strings to double values.  Given a string,
        /// such a function can either return a double (meaning that the string maps to the
        /// double) or throw an ArgumentException (meaning that the string is unmapped.
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
        {   //establish stacks value for the value tokens and operate for the operator tokens.
            Stack<double> value = new Stack<double>();
            Stack<string> operate = new Stack<string>();

            foreach(string token in tokens)
            {
                //if current token is a double.
                double num;
                if (double.TryParse(token, out num))
                {
                    //push token to value if operate is empty.
                    if (operate.Count == 0)
                    {
                        value.Push(num);
                        continue;
                    }

                    //if operate contains * or / on top, pop value, pop operate, apply the operator to
                    //token and popped value.
                    if (operate.Peek().Equals("*") || operate.Peek().Equals("/"))
                    {
                        double temp = 0;
                        double val = value.Pop();
                        string op = operate.Pop();
                        if (op.Equals("*"))
                            temp = val * num;
                        else
                            temp = val / num;
                        value.Push(temp);
                        continue;
                    }
                    //otherwise push the token onto value stack.
                    else
                    {
                        value.Push(num);
                        continue;
                    }
                }

                //if current token is a variable
                if(Regex.IsMatch(token, @"[a-zA-Z]+\d+"))
                {  
                    //try to lookup the value of the variable.
                    try
                    {
                        double temp = lookup(token);
                    }
                    catch(ArgumentException)
                    {   //Throw a more specific Exception than is thrown by a failed lookup.
                        throw new FormulaEvaluationException("Unable to find a value for all variables with method passed by user");   
                    }
                   //if lookup was succesful, store the variables value in tok.
                    double tok = lookup(token);
                   
                    //push to value if operate is empty.
                    if (operate.Count == 0)
                    {
                        value.Push(tok);
                        continue;
                    }
                    //if operate contains * or / on top of stack, pop value, pop operate, apply
                    //apply popped operator to popped value and tok. Push result to value.
                    if (operate.Peek().Equals("*") || operate.Peek().Equals("/"))
                    {
                        double temp = 0;
                        double val = value.Pop();
                        string op = operate.Pop();
                        if (op.Equals("*"))
                            temp = val * tok;
                        else
                            temp = val / tok;

                        value.Push(temp);
                        continue;
                    }
                    
                    else
                    {
                        value.Push(tok);
                        continue;
                    }
                }

                //if token is + or -, push it to operate it's empty.
                if(token.Equals("+") || token.Equals("-"))
                {
                    if(operate.Count == 0)
                    {
                        operate.Push(token);
                        continue;
                    }
                    //if top of operate is another + or -, double pop value, pop operate, apply the popped 
                    //operator to the 2 popped values. Push the result to value.
                    if(operate.Peek().Equals("+") || operate.Peek().Equals("-"))
                    {
                        double temp = 0;
                        double val1 = value.Pop(); 
                        double val2 = value.Pop();
                        string op = operate.Pop();
                        if(op.Equals("+"))
                            temp = val2 + val1;
                        else
                            temp = val2 - val1;
                       
                        value.Push(temp);
                    }
                    
                    //push the token to operate.
                    operate.Push(token);
                    continue;
                }

                //if the token is * / or ( push it to operate.
                if (token.Equals("*") || token.Equals("/") || token.Equals("("))
                {
                    operate.Push(token);
                    continue;
                }

                //if the token is a closed parenthesis
                if(token.Equals(")"))
                {
                    //if + or - is on top of operate, double pop value, pop operate, apply the popped operator
                    //to the 2 popped values.
                    double temp = 0;
                    if(operate.Peek().Equals("+") || operate.Peek().Equals("-"))
                    {
                        double val1 = value.Pop();
                        double val2 = value.Pop();
                        string op = operate.Pop();
                        if (op.Equals("+"))
                            temp = val2 + val1;
                        else
                            temp = val2 - val1;
                        value.Push(temp);
                        
                    }

                    string openparen = operate.Pop();
                    if (operate.Count == 0)
                        continue;
                    if(operate.Peek().Equals("*") || operate.Peek().Equals("/"))
                    {
                        double val1 = value.Pop();
                        double val2 = value.Pop();
                        string op = operate.Pop();
                        if (op.Equals("*"))
                            temp = val1 * val2;
                        else
                            temp = val2 / val1;
                        value.Push(temp); 
                    }
                }

            }

            if (operate.Count == 0)
                return value.Pop();
            else
            {
                double val1 = value.Pop();
                double val2 = value.Pop();
                string op = operate.Pop();

                if (op.Equals("+"))
                    return val2 + val1;
                else
                    return val2 - val1;

            }
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
